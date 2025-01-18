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
    internal abstract class NetworkProtocolBase
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public OpenAiConfig OpenAiConfig { get; set; }
        public ILog log;

        public bool IsMuted { get; set; } = false;

        public event EventHandler<EventArgs> SpeechStarted;
        public event EventHandler<AudioEventArgs> SpeechDataAvailable;
        public event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
        public event EventHandler<AudioEventArgs> SpeechEnded;

        public event EventHandler<EventArgs> PlaybackStarted;
        public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
        public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
        public event EventHandler<EventArgs> PlaybackEnded;


        public NetworkProtocolBase(OpenAiConfig openAiConfig, ILog ilog)
        {
            this.OpenAiConfig = openAiConfig;
            this.log = ilog;
        }

        protected string GetOpenAIRequestUrl()
        {
            return $"{OpenAiConfig.OpenApiUrl.TrimEnd('/').TrimEnd('?')}?model={OpenAiConfig.Model}";
        }

        protected string GetOpenAIRTCRequestUrl()
        {
            return $"{OpenAiConfig.OpenApiRtcUrl.TrimEnd('/').TrimEnd('?')}?model={OpenAiConfig.Model}";
        }

        protected string GetAuthorization()
        {
            if (string.IsNullOrEmpty(OpenAiConfig.ApiKey))
            {
                throw new InvalidOperationException("Invalid API Key.");
            }

            string authorization = OpenAiConfig.ApiKey;
            if (!OpenAiConfig.ApiKey.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
            {
                authorization = $"Bearer {OpenAiConfig.ApiKey}";
            }

            return authorization;
        }

        public Task ConnectAsync(SessionConfiguration sessionConfiguration, Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>> functionRegistries)
        {
            return ConnectAsyncCor(sessionConfiguration, functionRegistries);
        }
        protected abstract Task ConnectAsyncCor(SessionConfiguration sessionConfiguration, Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>> functionRegistries);


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

        protected virtual void OnSpeechStarted(EventArgs e)
        {
            SpeechStarted?.Invoke(this, e);
        }
        protected virtual void OnSpeechEnded(AudioEventArgs e)
        {
            SpeechEnded?.Invoke(this, e);
        }
        protected virtual void OnPlaybackDataAvailable(AudioEventArgs e)
        {
            PlaybackDataAvailable?.Invoke(this, e);
        }
        protected virtual void OnSpeechDataAvailable(AudioEventArgs e)
        {
            SpeechDataAvailable?.Invoke(this, e);
        }
        protected virtual void OnSpeechActivity(bool isActive, AudioEventArgs? audioArgs = null)
        {
            if (isActive)
            {
                SpeechStarted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SpeechEnded?.Invoke(this, audioArgs ?? new AudioEventArgs(new byte[0]));
            }
        }
        protected virtual void OnPlaybackStarted(EventArgs e)
        {
            PlaybackStarted?.Invoke(this, e);
        }
        protected virtual void OnPlaybackEnded(EventArgs e)
        {
            PlaybackEnded?.Invoke(this, e);
        }

        protected virtual void OnPlaybackTextAvailable(TranscriptEventArgs e)
        {
            PlaybackTextAvailable?.Invoke(this, e);
        }
        protected virtual void OnSpeechTextAvailable(TranscriptEventArgs e)
        {
            SpeechTextAvailable?.Invoke(this, e);
        }
    }
}
