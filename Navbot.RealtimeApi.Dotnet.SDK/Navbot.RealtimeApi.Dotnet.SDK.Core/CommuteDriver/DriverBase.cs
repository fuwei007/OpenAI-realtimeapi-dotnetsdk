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
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver
{
    internal abstract class DriverBase : ICommuteDriver
    {
        public event EventHandler<DataReceivedEventArgs> ReceivedDataAvailable;
        public ILog log;
        public string OpenApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string Model { get; set; }

        public Dictionary<string, string> RequestHeaderOptions { get; }


        public DriverBase(string apiKey, string openApiUrl, string model, Dictionary<string, string> RequestHeaderOptions, ILog ilog)
        {
            this.log = ilog;
            this.ApiKey = apiKey;

            this.OpenApiUrl = openApiUrl;
            this.Model = model;
            this.RequestHeaderOptions = RequestHeaderOptions;
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
        public Task ConnectAsync()
        {
            return ConnectAsyncCor();
        }
        protected abstract Task ConnectAsyncCor();
        

        public Task DisconnectAsync()
        {
            return DisconnectAsyncCor();
        }

        protected abstract Task DisconnectAsyncCor();


        public Task SendDataAsync(byte[]? messageBytes)
        {
            return SendDataAsyncCor(messageBytes);
        }
        protected abstract Task SendDataAsyncCor(byte[]? messageBytes);

        public Task CommitAudioBufferAsync()
        {
            return CommitAudioBufferAsyncCor();
        }
        protected abstract Task CommitAudioBufferAsyncCor();

        public Task ReceiveMessages()
        {
            return ReceiveMessagesCor();
        }
        protected abstract Task ReceiveMessagesCor();

        protected virtual void OnReceivedDataAvailable(DataReceivedEventArgs e)
        {
            ReceivedDataAvailable?.Invoke(this, e);
        }
    }
}
