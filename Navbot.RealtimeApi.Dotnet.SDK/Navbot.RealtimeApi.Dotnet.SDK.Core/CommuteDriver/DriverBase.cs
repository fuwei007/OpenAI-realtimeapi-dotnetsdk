using log4net;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Request;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver
{
    public class DriverBase
    {
        public ILog log;
        public string OpenApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string Model { get; set; }
        
        public Dictionary<string, string> RequestHeaderOptions { get; }


        public DriverBase(string apiKey, ILog ilog)
        {
            this.log = ilog;
            this.ApiKey = apiKey;
           
            this.OpenApiUrl = "wss://api.openai.com/v1/realtime";
            this.Model = "gpt-4o-realtime-preview-2024-10-01";
            this.RequestHeaderOptions = new Dictionary<string, string>();
            RequestHeaderOptions.Add("openai-beta", "realtime=v1");
        }

        protected string GetOpenAIRequestUrl()
        {
            return $"{this.OpenApiUrl.TrimEnd('/').TrimEnd('?')}?model={this.Model}";
        }

        protected string GetAuthorization()
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                throw new InvalidOperationException("Invalid API Key.");
            }

            string authorization = ApiKey;
            if (!ApiKey.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
            {
                authorization = $"Bearer {ApiKey}";
            }

            return authorization;
        }

       

       
    }
}
