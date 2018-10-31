﻿using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Devices.Tpm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace Edison.Devices.Common
{
    #pragma warning disable 1998
    public class AppIoTBackgroundDeviceTask
    {
        /// <summary>
        /// Next time that the device need to ping IoT Hub
        /// </summary>
        private DateTime _nextPingTime = DateTime.MinValue;
        /// <summary>
        /// Switch to interrupt and restart the application
        /// </summary>
        private bool _interruptApplication;
        /// <summary>
        /// Switch to confirm that the configuration was loaded once
        /// </summary>
        private bool _configurationLoaded;
        /// <summary>
        /// Logging helper
        /// </summary>
        private LoggingChannel _logging;
        /// <summary>
        /// IoT Hub helper
        /// </summary>
        private AzureIoTHubService _azureIoTHubService;
        /// <summary>
        /// GPIO helper
        /// </summary>
        private GPIOService _gpioService;
        /// <summary>
        /// Id of the application
        /// </summary>
        private readonly Guid _loggingAppId = Guid.Empty;
        /// <summary>
        /// Name of the application
        /// </summary>
        private readonly string _loggingAppName = "IoTApp";
        /// <summary>
        /// Ping interval to IoT Hub
        /// </summary>
        private readonly int _pingIntervalSecond = 60;
        /// <summary>
        /// Event type for test direct method
        /// </summary>
        private readonly string _eventTypeTest = "test";
        /// <summary>
        /// Hook for Init Application 
        /// </summary>
        private Func<LoggingChannel, AzureIoTHubService, GPIOService, bool> _funcInitApplication;
        /// <summary>
        /// Hook for Start Application 
        /// </summary>
        private Func<Task> _funcStartApplication;
        /// <summary>
        /// Hook for Run Application loop
        /// </summary>
        private Func<Task> _funcRunApplication;
        /// <summary>
        /// Hook for End Application 
        /// </summary>
        private Func<Task> _funcEndApplication;
        /// <summary>
        /// Hook for Disconnected 
        /// </summary>
        private Func<Task> _funcDisconnectedApplication;
        /// <summary>
        /// Hook for General Error in loop 
        /// </summary>
        private Func<Task> _funcGeneralError;
        /// <summary>
        /// Hook for Change Configuration 
        /// </summary>
        private Func<TwinCollection, Task> _funcChangeConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="appId"></param>
        public AppIoTBackgroundDeviceTask(string appName, Guid appId)
        {
            _loggingAppName = appName;
            _loggingAppId = appId;
        }

        /// <summary>
        /// IBackgroundTask starting point. Initializes logging
        /// </summary>
        /// <param name="taskInstance">IBackgroundTaskInstance object</param>
        public async Task Run()
        {
            //Create services
            _logging = new LoggingChannel(_loggingAppName, null, _loggingAppId);
            _azureIoTHubService = new AzureIoTHubService(_loggingAppId);
            _azureIoTHubService.SetDesiredPropertyCallback(ReceiveDesiredConfiguration);
            _gpioService = new GPIOService(_loggingAppId);

            //First time configuration (runs only once)
            _logging.LogMessage("Initializing Application", LoggingLevel.Verbose);
            if (!InitApplicationInternal())
                return;

            //Run application (loop indefinitely)
            _logging.LogMessage("Run Application", LoggingLevel.Verbose);
            await RunApplicationInternal();
        }

        private async Task RunApplicationInternal()
        {
            //Application phase
            try
            {
                //Application Loop
                _logging.LogMessage("Entering Application Loop", LoggingLevel.Verbose);
                _interruptApplication = false;
                while (!_interruptApplication)
                {
                    _logging.LogMessage("Connecting to IoT Hub", LoggingLevel.Verbose);
                    //Ensure connection
                    if (await _azureIoTHubService.Init())
                    {
                        //Start application hook
                        _logging.LogMessage("Starting Application", LoggingLevel.Verbose);
                        if (_funcStartApplication != null)
                            await _funcStartApplication();

                        //Main Loop
                        _logging.LogMessage("Entering Logic Loop", LoggingLevel.Verbose);
                        while (!_interruptApplication && _azureIoTHubService.Connected)
                        {
                            if (_nextPingTime < DateTime.UtcNow)
                            {
                                await _azureIoTHubService.SendPingMessage();
                                _nextPingTime = DateTime.UtcNow.AddSeconds(_pingIntervalSecond);
                            }
                            //Run Application hook
                            if (_funcRunApplication != null)
                                await _funcRunApplication();
                            else
                                await Task.Delay(500);
                        }
                        if (_interruptApplication)
                            _logging.LogMessage("Leaving Logic Loop - Manual interruption", LoggingLevel.Verbose);
                        else if (!_azureIoTHubService.Connected)
                            _logging.LogMessage("Leaving Logic Loop - Disconnected", LoggingLevel.Verbose);
                        else
                            _logging.LogMessage("Leaving Logic Loop - Unknown reason", LoggingLevel.Critical);
                    }
                    else
                    {
                        _logging.LogMessage("The connection to IoT Hub did not succeed. Please make sure that the TPM service is properly set up on Device 0 and that the connection string is properly set up.", LoggingLevel.Error);
                        if (_funcDisconnectedApplication != null)
                            await _funcDisconnectedApplication();
                        else
                            await Task.Delay(500);
                    } 
                }
                _logging.LogMessage("Leaving Application Loop", LoggingLevel.Verbose);
            }
            catch (Exception e)
            {
                _logging.LogMessage($"General Running Error: '{e.Message}'", LoggingLevel.Critical);
                _logging.LogMessage($"Stacktrace: '{e.StackTrace}'", LoggingLevel.Critical);
                if (_funcGeneralError != null)
                {
                    try
                    {
                        await _funcGeneralError();
                    }
                    catch (Exception eg)
                    {
                        _logging.LogMessage($"Error in GeneralError hook: '{eg.Message}'", LoggingLevel.Error);
                    }
                }
            }

            //End application phase
            _logging.LogMessage("Exiting Application", LoggingLevel.Information);
            try
            {
                //End Application hook
                if (_funcEndApplication != null)
                    await _funcEndApplication();

                //Program interrupted, dispose the services
                if (_azureIoTHubService != null)
                    _azureIoTHubService.Dispose();
                if (_gpioService != null)
                    _gpioService.Dispose();
            }
            catch (Exception e)
            {
                _logging.LogMessage($"General EndApplication Error: '{e.Message}'", LoggingLevel.Critical);
                _logging.LogMessage($"Stacktrace: '{e.StackTrace}'", LoggingLevel.Critical);
            }

            //Restart
            _logging.LogMessage("Restarting the application in 5 seconds...", LoggingLevel.Information);
            await Task.Delay(5000);
            await RunApplicationInternal();
        }

        private bool InitApplicationInternal()
        {
            if (_configurationLoaded)
                return true;

            try
            {
                _configurationLoaded = _funcInitApplication != null ? _funcInitApplication(_logging, _azureIoTHubService, _gpioService) : true;
                _azureIoTHubService.SetDirectMethodCallback("test", DirectMethodTest);
                return true;
            }
            catch(Exception e)
            {
                _logging.LogMessage($"Unhandled Error: '{e.Message}'", LoggingLevel.Critical);
                _logging.LogMessage($"Stacktrace: '{e.StackTrace}'", LoggingLevel.Critical);
                _configurationLoaded = false;
            }
            return false;
        }

        protected void RestartApplication()
        {
            _interruptApplication = true;
        }

        protected async Task ReceiveDesiredConfiguration(TwinCollection desiredProperties, object userContext)
        {
            _logging.LogMessage("ReceiveDesiredConfiguration", LoggingLevel.Verbose);

            if (_funcChangeConfiguration != null)
                await _funcChangeConfiguration(desiredProperties);
            else
                RestartApplication();
        }

        private async Task<MethodResponse> DirectMethodTest(MethodRequest methodRequest, object userContext)
        {
            await _azureIoTHubService.SendIoTMessage(_eventTypeTest);
            return new MethodResponse(200);
        }

        public void SetInitApplication(Func<LoggingChannel, AzureIoTHubService, GPIOService, bool> method)
        {
            _funcInitApplication = method;
        }

        public void SetStartApplication(Func<Task> method)
        {
            _funcStartApplication = method;
        }

        public void SetRunApplicationLoop(Func<Task> method)
        {
            _funcRunApplication = method;
        }

        public void SetEndApplication(Func<Task> method)
        {
            _funcEndApplication = method;
        }

        public void SetChangeConfiguration(Func<TwinCollection, Task> method)
        {
            _funcChangeConfiguration = method;
        }

        public void SetDisconnectedApplication(Func<Task> method)
        {
            _funcDisconnectedApplication = method;
        }

        public void SetGeneralError(Func<Task> method)
        {
            _funcGeneralError = method;
        }
    }
}
