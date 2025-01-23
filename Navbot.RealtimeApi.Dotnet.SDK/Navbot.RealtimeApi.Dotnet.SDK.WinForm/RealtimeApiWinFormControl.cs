using NAudio.CoreAudioApi;
using NAudio.Wave;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Enum;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Newtonsoft.Json.Linq;

namespace Navbot.RealtimeApi.Dotnet.SDK.WinForm
{
    public partial class RealtimeApiWinFormControl : UserControl
    {
        private WaveInEvent speechWaveIn;
        //private WasapiCapture capture;

        public event EventHandler<EventArgs> SpeechStarted;
        public event EventHandler<AudioEventArgs> SpeechDataAvailable;
        public event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
        public event EventHandler<EventArgs> SpeechEnded;

        public event EventHandler<EventArgs> PlaybackStarted;
        public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
        public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
        public event EventHandler<EventArgs> PlaybackEnded;

        public RealtimeApiWinFormControl()
        {
            InitializeComponent();
            RealtimeApiSdk = new RealtimeApiSdk();
            this.VoiceVisualEffect = VoiceVisualEffect.SoundWave;

            this.Resize += (s, e) => this.Invalidate();
        }


        public RealtimeApiSdk RealtimeApiSdk { get; private set; }

        public string OpenAiApiKey
        {
            get { return RealtimeApiSdk.ApiKey; }
            set { RealtimeApiSdk.ApiKey = value; }
        }
        public NetworkProtocolType NetworkProtocolType
        {
            get { return RealtimeApiSdk.NetworkProtocolType; }
            set { RealtimeApiSdk.NetworkProtocolType = value; }
        }

        public SessionConfiguration SessionConfiguration
        {
            get { return RealtimeApiSdk.SessionConfiguration; }
        }

        public VoiceVisualEffect VoiceVisualEffect
        {
            get
            {
                VoiceVisualEffect rtn = VoiceVisualEffect.Cycle;
                switch (audioVisualizer.VisualEffect)
                {
                    case AudioVisualizer.Core.Enum.VisualEffect.Oscilloscope:
                        rtn = VoiceVisualEffect.Oscilloscope;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.SpectrumBar:
                        rtn = VoiceVisualEffect.SoundWave;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle:
                        rtn = VoiceVisualEffect.Cycle;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.Border:
                        rtn = VoiceVisualEffect.Border;
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
                    case VoiceVisualEffect.Cycle:
                        audioVisualizer.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle;
                        break;
                    case VoiceVisualEffect.SoundWave:
                        audioVisualizer.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumBar;
                        break;
                    case VoiceVisualEffect.Oscilloscope:
                        audioVisualizer.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.Oscilloscope;
                        break;
                    case VoiceVisualEffect.Border:
                        audioVisualizer.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.Border;
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
                //capture.StartRecording();
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
                //capture.StopRecording();
                audioVisualizer.Stop();
                speechWaveIn.StopRecording();

                // Stop voice recognition;
                RealtimeApiSdk.StopSpeechRecognitionAsync();
            }
        }

        private void RealtimeApiDesktopControl_Load(object sender, EventArgs e)
        {
            //capture = new WasapiLoopbackCapture()
            //{
            //    WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1)
            //};
            //capture.DataAvailable += Audio_DataAvailable;

            speechWaveIn = new WaveInEvent
            {
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1)
            };

            speechWaveIn.DataAvailable += Audio_DataAvailable;

            // Raise event from sdk
            RealtimeApiSdk.SpeechStarted += (s, e) => { SpeechStarted?.Invoke(this, e); };
            RealtimeApiSdk.SpeechDataAvailable += (s, e) => { SpeechDataAvailable?.Invoke(this, e); };
            RealtimeApiSdk.SpeechTextAvailable += (s, e) => { SpeechTextAvailable?.Invoke(this, e); };
            RealtimeApiSdk.SpeechEnded += (s, e) => { SpeechEnded?.Invoke(this, e); };

            RealtimeApiSdk.PlaybackStarted += (s, e) => { PlaybackStarted?.Invoke(this, e); };
            //RealtimeApiSdk.PlaybackDataAvailable += (s, e) => { PlaybackDataAvailable?.Invoke(this, e); };
            RealtimeApiSdk.PlaybackDataAvailable += RealtimeApiSdk_PlaybackDataAvailable;
            RealtimeApiSdk.PlaybackTextAvailable += (s, e) => { PlaybackTextAvailable?.Invoke(this, e); };
            RealtimeApiSdk.PlaybackEnded += (s, e) => { PlaybackEnded?.Invoke(this, e); };

            //audioVisualizer.AudioSampleRate = capture.WaveFormat.SampleRate;
            audioVisualizer.Scale = 5;
        }

        private void RealtimeApiSdk_PlaybackDataAvailable(object? sender, AudioEventArgs e)
        {
            PlaybackDataAvailable?.Invoke(this, e);

            byte[] iEEEAudioBytes = e.GetIEEEAudioBuffer();
            Audio_DataAvailable(null, new WaveInEventArgs(iEEEAudioBytes, iEEEAudioBytes.Length));
        }

        public void RegisterFunctionCall(FunctionCallSetting functionCallSetting, Func<FuncationCallArgument, JObject> functionCallback)
        {
            RealtimeApiSdk.RegisterFunctionCall(functionCallSetting, functionCallback);
        }

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
