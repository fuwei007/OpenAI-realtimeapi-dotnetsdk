﻿using Newtonsoft.Json;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response
{
    /// <summary>
    /// conversation.created
    /// </summary>
    public class ConversationCreated : BaseResponse
    {
        [JsonProperty("conversation")]
        public Conversation Conversation { get; set; }
    }

    public class Conversation
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }
    }
}
