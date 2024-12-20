﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realtime.API.Dotnet.SDK.Core.Model.Function
{
    public class FunctionCallResult
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("item")]
        public FunctionCallItem Item { get; set; }
    }

    public class FunctionCallItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("output")]
        public string Output { get; set; }

        [JsonProperty("call_id")]
        public string CallId { get; set; }
    }

    public class ResponseJson
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
