using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using NAudio.CoreAudioApi;
using AudioVisualizer.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;

namespace Navbot.RealtimeApi.Dotnet.SDK.WinForm
{
    public partial class RealtimeApiWinFormControl : UserControl
    {
        private const string apiKey = "";

        private WaveInEvent speechWaveIn;
        private WasapiCapture capture;

        public event EventHandler<WaveInEventArgs> WaveInDataAvailable;
        public event EventHandler<WebSocketResponseEventArgs> WebSocketResponse;

        public event EventHandler<EventArgs> SpeechStarted;
        public event EventHandler<AudioEventArgs> SpeechDataAvailable;
        public event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
        public event EventHandler<AudioEventArgs> SpeechEnded;

        public event EventHandler<EventArgs> PlaybackStarted;
        public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
        public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
        public event EventHandler<EventArgs> PlaybackEnded;

        public RealtimeApiWinFormControl()
        {
            InitializeComponent();
            RealtimeApiSdk = new RealtimeApiSdk();

            this.Resize += (s, e) => this.Invalidate();
        }


        public RealtimeApiSdk RealtimeApiSdk { get; private set; }

        public string OpenAiApiKey
        {
            get { return RealtimeApiSdk.ApiKey; }
            set { RealtimeApiSdk.ApiKey = value; }
        }

        public SessionConfiguration SessionConfiguration
        {
            get { return RealtimeApiSdk.SessionConfiguration; }
        }

        public VisualEffect VoiceVisualEffect
        {
            get
            {
                VisualEffect rtn = VisualEffect.Cycle;
                switch (audioVisualizer.VisualEffict)
                {
                    case VisualEffict.Oscilloscope:
                        rtn = VisualEffect.Cycle;
                        break;
                    case VisualEffict.SpectrumBar:

                        break;
                    case VisualEffict.SpectrumCycle:
                        rtn = VisualEffect.Cycle;
                        break;
                    default:
                        break;
                }

                return rtn;
            }
            set
            {
                switch (value)
                {
                    case VisualEffect.Cycle:
                        audioVisualizer.VisualEffict = VisualEffict.SpectrumCycle;
                        break;
                    case VisualEffect.SoundWave:
                        audioVisualizer.VisualEffict = VisualEffict.SpectrumBar;
                        break;
                    default:
                        break;
                }
            }
        }


        public void StartSpeechRecognition()
        {
            if (!RealtimeApiSdk.IsRunning)
            {
                // Start ripple effect.
                capture.StartRecording();
                audioVisualizer.Start();
                speechWaveIn.StartRecording();

                // Start voice recognition;
                RealtimeApiSdk.StartSpeechRecognitionAsync();
            }
        }

        public void StopSpeechRecognition()
        {
            if (RealtimeApiSdk.IsRunning)
            {
                // Stop the ripple effect.
                capture.StopRecording();
                audioVisualizer.Stop();
                speechWaveIn.StopRecording();

                // Stop voice recognition;
                RealtimeApiSdk.StopSpeechRecognitionAsync();
            }
        }

        private void RealtimeApiDesktopControl_Load(object sender, EventArgs e)
        {
            capture = new WasapiLoopbackCapture()
            {
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1)
            };
            capture.DataAvailable += Audio_DataAvailable;

            speechWaveIn = new WaveInEvent
            {
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1)
            };

            speechWaveIn.DataAvailable += Audio_DataAvailable;

            RealtimeApiSdk.WebSocketResponse += RealtimeApiSdk_WebSocketResponse;

            RealtimeApiSdk.SpeechStarted += RealtimeApiSdk_SpeechStarted;
            RealtimeApiSdk.SpeechDataAvailable += RealtimeApiSdk_SpeechDataAvailable;
            RealtimeApiSdk.SpeechTextAvailable += RealtimeApiSdk_SpeechTextAvailable;
            RealtimeApiSdk.SpeechEnded += RealtimeApiSdk_SpeechEnded;

            RealtimeApiSdk.PlaybackStarted += RealtimeApiSdk_PlaybackStarted;
            RealtimeApiSdk.PlaybackDataAvailable += RealtimeApiSdk_PlaybackDataAvailable;
            RealtimeApiSdk.PlaybackTextAvailable += RealtimeApiSdk_PlaybackTextAvailable;
            RealtimeApiSdk.PlaybackEnded += RealtimeApiSdk_PlaybackEnded;

            audioVisualizer.AudioSampleRate = capture.WaveFormat.SampleRate;
            audioVisualizer.VisualEffict = VisualEffict.Oscilloscope;
            audioVisualizer.Scale = 5;
        }

        #region Event

        public void RegisterFunctionCall(FunctionCallSetting functionCallSetting, Func<FuncationCallArgument, JObject> functionCallback)
        {
            RealtimeApiSdk.RegisterFunctionCall(functionCallSetting, functionCallback);
        }
        private void RealtimeApiSdk_PlaybackEnded(object? sender, EventArgs e)
        {
            OnPlaybackEnded(e);
        }

        protected virtual void OnPlaybackEnded(EventArgs e)
        {
            PlaybackEnded?.Invoke(this, e);
        }
        private void RealtimeApiSdk_PlaybackTextAvailable(object? sender, TranscriptEventArgs e)
        {
            OnPlaybackTextAvailable(e);
        }

        protected virtual void OnPlaybackTextAvailable(TranscriptEventArgs e)
        {
            PlaybackTextAvailable?.Invoke(this, e);
        }
        private void RealtimeApiSdk_PlaybackDataAvailable(object? sender, AudioEventArgs e)
        {
            OnPlaybackDataAvailable(e);
        }
        protected virtual void OnPlaybackDataAvailable(AudioEventArgs e)
        {
            PlaybackDataAvailable?.Invoke(this, e);
        }
        private void RealtimeApiSdk_PlaybackStarted(object? sender, EventArgs e)
        {
            OnPlaybackStarted(e);
        }

        protected virtual void OnPlaybackStarted(EventArgs e)
        {
            PlaybackStarted?.Invoke(this, e);
        }
        private void RealtimeApiSdk_SpeechEnded(object? sender, AudioEventArgs e)
        {
            OnSpeechEnded(e);
        }

        protected virtual void OnSpeechEnded(AudioEventArgs e)
        {
            SpeechEnded?.Invoke(this, e);
        }
        private void RealtimeApiSdk_SpeechTextAvailable(object? sender, TranscriptEventArgs e)
        {
            OnSpeechTextAvailable(e);
        }

        protected virtual void OnSpeechTextAvailable(TranscriptEventArgs e)
        {
            SpeechTextAvailable?.Invoke(this, e);
        }
        private void RealtimeApiSdk_SpeechDataAvailable(object? sender, AudioEventArgs e)
        {
            OnSpeechDataAvailable(e);
        }

        protected virtual void OnSpeechDataAvailable(AudioEventArgs e)
        {
            SpeechDataAvailable?.Invoke(this, e);
        }

        private void RealtimeApiSdk_WebSocketResponse(object? sender, WebSocketResponseEventArgs e)
        {
            OnWebSocketResponse(e);
        }

        protected virtual void OnWebSocketResponse(WebSocketResponseEventArgs e)
        {
            WebSocketResponse?.Invoke(this, e);
        }
        private void RealtimeApiSdk_SpeechStarted(object? sender, EventArgs e)
        {
            OnSpeechStarted(e);
        }

        protected virtual void OnSpeechStarted(EventArgs e)
        {
            SpeechStarted?.Invoke(this, e);
        }
        #endregion


        private void Audio_DataAvailable(object? sender, WaveInEventArgs e)
        {
            int length = e.BytesRecorded / 4;           // Float data
            double[] result = new double[length];

            for (int i = 0; i < length; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4);

            // Push into visualizer
            audioVisualizer.PushSampleData(result);

        }


    }
}
