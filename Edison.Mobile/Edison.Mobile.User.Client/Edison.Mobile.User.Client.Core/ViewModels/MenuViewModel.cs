﻿using System.Threading.Tasks;
using Edison.Mobile.Common.Auth;
using Edison.Mobile.Common.Shared;

namespace Edison.Mobile.User.Client.Core.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        readonly AuthService authService;

        public string ProfileName => authService.Email;

        public MenuViewModel(AuthService authService)
        {
            this.authService = authService;
        }

        public async Task SignOut()
        {
            await authService.SignOut();
        }
    }
}
