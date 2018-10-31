﻿using Microsoft.Bot.Schema;
using System;
using Newtonsoft.Json;
using Edison.ChatService.Helpers;

namespace Edison.ChatService.Models
{
    [Serializable]
    public class Connection : IEquatable<Connection>
    {
        /// <summary>
        /// Represents the last time in which user and agent interact in the connection.
        /// </summary>
        public DateTime TimeSinceLastActivity
        {
            get;
            set;
        }

        public ConversationReference ConversationReference1
        {
            get;
            set;
        }

        public ConversationReference ConversationReference2
        {
            get;
            set;
        }

        public Connection(ConversationReference conversationReference1, ConversationReference conversationReference2)
        {
            ConversationReference1 = conversationReference1;
            ConversationReference2 = conversationReference2;
            TimeSinceLastActivity = DateTime.Now;
        }

        /// <summary>
        /// Checks if the given connection matches this one.
        /// </summary>
        /// <param name="other">The other connection.</param>
        /// <returns>True, if the connections are match. False otherwise.</returns>
        public bool Equals(Connection other)
        {
            return (other != null
                && ((ConversationHelper.Match(ConversationReference1, other.ConversationReference1)
                     && ConversationHelper.Match(ConversationReference2, other.ConversationReference2))
                     || (ConversationHelper.Match(ConversationReference1, other.ConversationReference2)
                         && ConversationHelper.Match(ConversationReference2, other.ConversationReference1))));
        }

        public static Connection FromJson(string connectionAsJsonString)
        {
            Connection connection = null;

            try
            {
                connection = JsonConvert.DeserializeObject<Connection>(connectionAsJsonString);
            }
            catch (Exception)
            {
            }

            return connection;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
