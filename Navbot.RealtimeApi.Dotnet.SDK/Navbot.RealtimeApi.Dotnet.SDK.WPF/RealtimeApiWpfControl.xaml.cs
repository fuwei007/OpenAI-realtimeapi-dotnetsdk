using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using System.Windows;
using System.Windows.Controls;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using System.ComponentModel;
using NAudio.CoreAudioApi;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class RealtimeApiWpfControl : UserControl, INotifyPropertyChanged
    {
        //TODO2 Move into Api Sdk
        private WaveInEvent speechWaveIn;

        // TODO2
        private WasapiLoopbackCapture speakerCapture;
        private BufferedWaveProvider speakerWaveProvider;

        public event EventHandler<EventArgs> SpeechStarted;
        public event EventHandler<AudioEventArgs> SpeechDataAvailable;
        public event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
        public event EventHandler<AudioEventArgs> SpeechEnded;

        public event EventHandler<EventArgs> PlaybackStarted;
        public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
        public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
        public event EventHandler<EventArgs> PlaybackEnded;

        private WasapiCapture capture;

        public IReadOnlyList<ConversationEntry> ConversationEntries => RealtimeApiSdk.ConversationEntries;
        public string ConversationAsText
        {
            get => RealtimeApiSdk.ConversationAsText;
        }

        public RealtimeApiWpfControl()
        {
            InitializeComponent();

            RealtimeApiSdk = new RealtimeApiSdk();
            this.VoiceVisualEffect = WPF.VisualEffect.SoundWave;

            Loaded += RealtimeApiWpfControl_Loaded;
            RealtimeApiSdk.SpeechTextAvailable += OnConversationUpdated;
            RealtimeApiSdk.PlaybackTextAvailable += OnConversationUpdated;
        }

        public RealtimeApiSdk RealtimeApiSdk { get; private set; }
        public NetworkProtocolType NetworkProtocolType
        {
            get { return RealtimeApiSdk.NetworkProtocolType; }
            set { RealtimeApiSdk.NetworkProtocolType = value; }
        }
        public string OpenAiApiKey
        {
            get { return RealtimeApiSdk.ApiKey; }
            set { RealtimeApiSdk.ApiKey = value; }
        }

        public VisualEffect VoiceVisualEffect
        {
            get
            {
                VisualEffect rtn = WPF.VisualEffect.Cycle;
                switch (audioVisualizerView.VisualEffect)
                {
                    case AudioVisualizer.Core.Enum.VisualEffect.Oscilloscope:
                        rtn = WPF.VisualEffect.Oscilloscope;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.SpectrumBar:
                        rtn = WPF.VisualEffect.SoundWave;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle:
                        rtn = WPF.VisualEffect.Cycle;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.Border:
                        rtn = WPF.VisualEffect.Border;
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
                    case WPF.VisualEffect.Cycle:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle;
                        break;
                    case WPF.VisualEffect.SoundWave:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumBar;
                        break;
                    case WPF.VisualEffect.Oscilloscope:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.Oscilloscope;
                        break;
                    case WPF.VisualEffect.Border:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.Border;
                        break;
                    default:
                        break;
                }
            }
        }

        public SessionConfiguration SessionConfiguration
        {
            get { return RealtimeApiSdk.SessionConfiguration; }
        }

        public bool ReactToMicInput { get; set; } = false;

        private void RealtimeApiWpfControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is FrameworkElement parent)
            {
                parent.SizeChanged += Parent_SizeChanged;
                UpdateControlSize(parent);
            }
        }

        private void Parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is FrameworkElement parent)
            {
                UpdateControlSize(parent);
            }
        }

        private void UpdateControlSize(FrameworkElement parent)
        {
            this.Width = parent.ActualWidth;
            this.Height = parent.ActualHeight;
            this.UpdateLayout();
        }

        protected virtual void OnPlaybackDataAvailable(AudioEventArgs e)
        {
            PlaybackDataAvailable?.Invoke(this, e);
        }

        private void RealtimeApiSdk_PlaybackEnded(object? sender, EventArgs e)
        {
            OnPlaybackEnded(e);
        }

        protected virtual void OnPlaybackEnded(EventArgs e)
        {
            PlaybackEnded?.Invoke(this, e);
        }

        private void RealtimeApiSdk_PlaybackStarted(object? sender, EventArgs e)
        {
            OnPlaybackStarted(e);
        }

        protected virtual void OnPlaybackStarted(EventArgs e)
        {
            PlaybackStarted?.Invoke(this, e);
        }

        private void RealtimeApiSdk_PlaybackTextAvailable(object? sender, TranscriptEventArgs e)
        {
            OnPlaybackTextAvailable(e);
        }

        protected virtual void OnPlaybackTextAvailable(TranscriptEventArgs e)
        {
            PlaybackTextAvailable?.Invoke(this, e);
        }

        private void RealtimeApiSdk_SpeechEnded(object? sender, AudioEventArgs e)
        {
            OnSpeechEnded(e);
        }

        protected virtual void OnSpeechEnded(AudioEventArgs e)
        {
            SpeechEnded?.Invoke(this, e);
        }

        private void RealtimeApiSdk_SpeechStarted(object? sender, EventArgs e)
        {
            OnSpeechStarted(e);
        }

        protected virtual void OnSpeechStarted(EventArgs e)
        {
            SpeechStarted?.Invoke(this, e);
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Not executing during design pattern
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return; 
            }

            speakerCapture = new WasapiLoopbackCapture();
            speakerWaveProvider = new BufferedWaveProvider(speakerCapture.WaveFormat)
            {
                BufferLength = 1024 * 1024, // 1 MB buffer (adjust based on your needs)
                DiscardOnBufferOverflow = true // Optional: discard data when buffer is full}
            };

            speakerCapture.DataAvailable += SpeakerCapture_DataAvailable;

            RealtimeApiSdk.SpeechStarted += RealtimeApiSdk_SpeechStarted;
            RealtimeApiSdk.SpeechDataAvailable += RealtimeApiSdk_SpeechDataAvailable;
            RealtimeApiSdk.SpeechTextAvailable += RealtimeApiSdk_SpeechTextAvailable;
            RealtimeApiSdk.SpeechEnded += RealtimeApiSdk_SpeechEnded;

            RealtimeApiSdk.PlaybackStarted += RealtimeApiSdk_PlaybackStarted;
            RealtimeApiSdk.PlaybackDataAvailable += RealtimeApiSdk_PlaybackDataAvailable;
            RealtimeApiSdk.PlaybackTextAvailable += RealtimeApiSdk_PlaybackTextAvailable;
            RealtimeApiSdk.PlaybackEnded += RealtimeApiSdk_PlaybackEnded;

            audioVisualizerView.AudioSampleRate = speakerCapture.WaveFormat.SampleRate;
            audioVisualizerView.Scale = 5;

            audioVisualizerView.StartRenderAsync();
        }


        private void RealtimeApiSdk_PlaybackDataAvailable(object? sender, AudioEventArgs e)
        {
            OnPlaybackDataAvailable(e);
        }

        public void StartSpeechRecognition()
        {
            if (!RealtimeApiSdk.IsRunning)
            {
                // Start voice recognition;
                RealtimeApiSdk.StartSpeechRecognitionAsync();
                ReactToMicInput = true;

                speechWaveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(44100, 1)
                };

                speechWaveIn.DataAvailable += SpeechWaveIn_DataAvailable;
                speechWaveIn.StartRecording();
            }
        }

        public void StopSpeechRecognition()
        {
            if (RealtimeApiSdk.IsRunning)
            {
                //Stop voice recognition;
                RealtimeApiSdk.StopSpeechRecognitionAsync();
                ReactToMicInput = false;

                speechWaveIn.StopRecording();
            }
        }

        public void RegisterFunctionCall(FunctionCallSetting functionCallSetting, Func<FuncationCallArgument, JObject> functionCallback)
        {
            RealtimeApiSdk.RegisterFunctionCall(functionCallSetting, functionCallback);
        }


        private void SpeechWaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (!ReactToMicInput)
            {
                // Ignore microphone input
                return;
            }

            List<float> audioBuffer = new List<float>();
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short value = BitConverter.ToInt16(e.Buffer, i);
                float normalized = value / 32768f;
                audioBuffer.Add(normalized);
            }
            double[] result = audioBuffer.Select(f => (double)f).ToArray();
            audioVisualizerView.PushSampleData(result);
        }

        private void SpeakerCapture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            speakerWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            var audioBuffer = new float[e.BytesRecorded / 4];
            WaveBuffer waveBuffer = new WaveBuffer(e.Buffer);
            for (int i = 0; i < audioBuffer.Length; i++)
            {
                audioBuffer[i] = waveBuffer.FloatBuffer[i];
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnConversationUpdated(object sender, TranscriptEventArgs e)
        {
            // Notify that the conversation data has changed
            RefreshConversationData();
        }

        public void RefreshConversationData()
        {
            NotifyPropertyChanged(nameof(ConversationEntries));
            NotifyPropertyChanged(nameof(ConversationAsText));
        }

        public void ClearConversationHistory()
        {
            RealtimeApiSdk.ClearConversationEntries();
            RefreshConversationData();
        }
    }
}
