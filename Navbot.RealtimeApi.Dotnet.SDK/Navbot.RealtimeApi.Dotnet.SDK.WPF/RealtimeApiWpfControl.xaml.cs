using NAudio.Wave;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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
        public event EventHandler<EventArgs> SpeechEnded;

        public event EventHandler<EventArgs> PlaybackStarted;
        public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
        public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
        public event EventHandler<EventArgs> PlaybackEnded;

        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyList<ConversationEntry> ConversationEntries => RealtimeApiSdk.ConversationEntries;
        public string ConversationAsText
        {
            get => RealtimeApiSdk.ConversationAsText;
        }

        public RealtimeApiWpfControl()
        {
            InitializeComponent();

            RealtimeApiSdk = new RealtimeApiSdk();
            this.VoiceVisualEffect = Core.VisualEffect.SoundWave;

            Loaded += RealtimeApiWpfControl_Loaded;
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
                VisualEffect rtn = Core.VisualEffect.Cycle;
                switch (audioVisualizerView.VisualEffect)
                {
                    case AudioVisualizer.Core.Enum.VisualEffect.Oscilloscope:
                        rtn = Core.VisualEffect.Oscilloscope;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.SpectrumBar:
                        rtn = Core.VisualEffect.SoundWave;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle:
                        rtn = Core.VisualEffect.Cycle;
                        break;
                    case AudioVisualizer.Core.Enum.VisualEffect.Border:
                        rtn = Core.VisualEffect.Border;
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
                    case Core.VisualEffect.Cycle:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle;
                        break;
                    case Core.VisualEffect.SoundWave:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumBar;
                        break;
                    case Core.VisualEffect.Oscilloscope:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.Oscilloscope;
                        break;
                    case Core.VisualEffect.Border:
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

        private void RealtimeApiWpfControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is FrameworkElement parent)
            {
                parent.SizeChanged += Parent_SizeChanged;
                UpdateControlSize(parent);
            }

            // Notify that the conversation data has changed
            RealtimeApiSdk.SpeechTextAvailable += (s, e) => { RefreshConversationData();  };
            RealtimeApiSdk.PlaybackTextAvailable += (s, e) => { RefreshConversationData(); };
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


            // Raise event from sdk
            RealtimeApiSdk.SpeechStarted += (s, e) => { SpeechStarted?.Invoke(this, e); };
            RealtimeApiSdk.SpeechDataAvailable += (s, e) => { SpeechDataAvailable?.Invoke(this, e); };
            RealtimeApiSdk.SpeechTextAvailable += (s, e) => { SpeechTextAvailable?.Invoke(this, e); };
            RealtimeApiSdk.SpeechEnded += (s, e) => { SpeechEnded?.Invoke(this, e); };

            RealtimeApiSdk.PlaybackStarted += (s, e) => { PlaybackStarted?.Invoke(this, e); };
            RealtimeApiSdk.PlaybackDataAvailable += (s, e) => { PlaybackDataAvailable?.Invoke(this, e); };
            RealtimeApiSdk.PlaybackTextAvailable += (s, e) => { PlaybackTextAvailable?.Invoke(this, e); };
            RealtimeApiSdk.PlaybackEnded += (s, e) => { PlaybackEnded?.Invoke(this, e); };

            audioVisualizerView.AudioSampleRate = speakerCapture.WaveFormat.SampleRate;
            audioVisualizerView.Scale = 5;

            audioVisualizerView.StartRenderAsync();
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

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
 
    }
}
