﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response
{
    /// <summary>
    /// session.updated
    /// </summary>
    public class SessionUpdate : BaseResponse
    {
        [JsonProperty("session")]
        public Session Session { get; set; }
    }
}
