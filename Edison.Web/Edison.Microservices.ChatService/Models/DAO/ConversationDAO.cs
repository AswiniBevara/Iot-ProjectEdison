﻿using Edison.ChatService.Repositories;
using Edison.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Edison.ChatService.Models.DAO
{
    public class ConversationDAO : IEntityChatDAO
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string ReportType { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public List<ConversationLogDAOObject> ConversationLogs { get; set; }
        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime CreationDate { get; set; }
        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime UpdateDate { get; set; }
        [JsonConverter(typeof(EpochDateTimeConverter))]
        public DateTime? EndDate { get; set; }
        public string ETag { get; set; }
    }
}
