using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity
{
    internal class OpenAiConfig
    {
        public string OpenApiUrl { get; set; }
        public string OpenApiRtcUrl { get; set; }
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public string Voice { get; set; }
        public Dictionary<string, string> RequestHeaderOptions { get; set; }

        public OpenAiConfig(string apiKey) 
        {
            this.ApiKey = apiKey;
            
            this.OpenApiUrl = "wss://api.openai.com/v1/realtime";
            this.OpenApiRtcUrl = "https://api.openai.com/v1/realtime";
            this.Model = "gpt-4o-realtime-preview-2024-12-17";
            this.Voice = "ash";
            this.RequestHeaderOptions = new Dictionary<string, string>();
            RequestHeaderOptions.Add("openai-beta", "realtime=v1");
        }
    }
}
