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
    internal abstract class NetworkProtocolBase : INetworkProtocol
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public ILog log;
        public string OpenApiUrl { get; set; }
        public string OpenApiRtcUrl { get; set; }
        public string ApiKey { get; set; }
        public string Model { get; set; }
        public string Voice { get; set; }

        public Dictionary<string, string> RequestHeaderOptions { get; }


        public NetworkProtocolBase(OpenAiConfig openAiConfig, ILog ilog)
        {
            this.log = ilog;
            this.ApiKey = openAiConfig.ApiKey;
            this.OpenApiUrl = openAiConfig.OpenApiUrl;
            this.OpenApiRtcUrl = openAiConfig.OpenApiRtcUrl;
            this.Model = openAiConfig.Model;
            this.Voice = openAiConfig.Voice;
            this.RequestHeaderOptions = openAiConfig.RequestHeaderOptions;
        }

        protected string GetOpenAIRequestUrl()
        {
            return $"{this.OpenApiUrl.TrimEnd('/').TrimEnd('?')}?model={this.Model}";
        }

        protected string GetOpenAIRTCRequestUrl()
        {
            return $"{this.OpenApiRtcUrl.TrimEnd('/').TrimEnd('?')}?model={this.Model}";
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

        public Task ConnectAsync(SessionConfiguration sessionConfiguration)
        {
            return ConnectAsyncCor(sessionConfiguration);
        }
        protected abstract Task ConnectAsyncCor(SessionConfiguration sessionConfiguration);


        public Task DisconnectAsync()
        {
            return DisconnectAsyncCor();
        }

        protected abstract Task DisconnectAsyncCor();


        public Task SendDataAsync(byte[] messageBytes)
        {
            return SendDataAsyncCor(messageBytes);
        }
        protected abstract Task SendDataAsyncCor(byte[] messageBytes);

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

        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, e);
        }


    }
}
