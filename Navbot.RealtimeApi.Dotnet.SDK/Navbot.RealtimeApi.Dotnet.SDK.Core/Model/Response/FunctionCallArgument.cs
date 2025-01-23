using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response
{
    /// <summary>
    /// response.function_call_arguments.done
    /// </summary>
    public class FunctionCallArgument : BaseResponse
    {
        [JsonProperty("response_id")]
        public string ResponseId { get; set; }
        [JsonProperty("item_id")]
        public string ItemId { get; set; }
        [JsonProperty("output_index")]
        public int OutputIndex { get; set; }
        [JsonProperty("call_id")]
        public string CallId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("arguments")]
        public string Arguments { get; set; }
    }
}
