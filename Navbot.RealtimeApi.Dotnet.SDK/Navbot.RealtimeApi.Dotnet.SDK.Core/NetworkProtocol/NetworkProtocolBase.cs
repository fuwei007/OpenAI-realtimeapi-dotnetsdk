using log4net;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Newtonsoft.Json.Linq;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver
{
    internal abstract class NetworkProtocolBase
    {
        //public event EventHandler<DataReceivedEventArgs> DataReceived;

        internal event EventHandler<EventArgs> SpeechStarted;
        internal event EventHandler<AudioEventArgs> SpeechDataAvailable;
        internal event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
        internal event EventHandler<EventArgs> SpeechEnded;

        internal event EventHandler<EventArgs> PlaybackStarted;
        internal event EventHandler<AudioEventArgs> PlaybackDataAvailable;
        internal event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
        internal event EventHandler<EventArgs> PlaybackEnded;


        internal NetworkProtocolBase(OpenAiConfig openAiConfig, ILog ilog)
        {
            this.OpenAiConfig = openAiConfig;
            this.Log = ilog;
        }
        internal bool IsMuted { get; set; } = false;

        protected ILog Log { get; private set; }

        protected OpenAiConfig OpenAiConfig { get; }
        
        protected abstract Task ConnectAsyncCor(SessionConfiguration sessionConfiguration, Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>> functionRegistries);

        protected abstract Task DisconnectAsyncCor();

        protected abstract Task SendDataAsyncCor(byte[] messageBytes);

        protected abstract Task ReceiveMessagesCor();

        protected abstract Task CommitAudioBufferAsyncCor();


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

        internal Task ConnectAsync(SessionConfiguration sessionConfiguration, Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>> functionRegistries)
        {
            return ConnectAsyncCor(sessionConfiguration, functionRegistries);
        }

        internal Task DisconnectAsync()
        {
            return DisconnectAsyncCor();
        }

        public Task SendDataAsync(byte[] messageBytes)
        {
            return SendDataAsyncCor(messageBytes);
        }

        public Task CommitAudioBufferAsync()
        {
            return CommitAudioBufferAsyncCor();
        }

        public Task ReceiveMessages()
        {
            return ReceiveMessagesCor();
        }

        protected virtual void OnSpeechStarted(EventArgs e)
        {
            SpeechStarted?.Invoke(this, e);
        }

        protected virtual void OnSpeechEnded(EventArgs e)
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

        protected void OnSpeechActivity(bool isActive)
        {
            if (isActive)
            {
                OnSpeechStarted(EventArgs.Empty);
            }
            else
            {
                OnSpeechEnded(EventArgs.Empty);
            }
        }
    }
}
