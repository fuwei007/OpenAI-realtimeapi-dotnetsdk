using NAudio.CoreAudioApi;
using NAudio.Wave;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Enum;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
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
        public event EventHandler<EventArgs> SpeechStarted;
        public event EventHandler<AudioEventArgs> SpeechDataAvailable;
        public event EventHandler<TranscriptEventArgs> SpeechTextAvailable;
        public event EventHandler<EventArgs> SpeechEnded;

        public event EventHandler<EventArgs> PlaybackStarted;
        public event EventHandler<AudioEventArgs> PlaybackDataAvailable;
        public event EventHandler<TranscriptEventArgs> PlaybackTextAvailable;
        public event EventHandler<EventArgs> PlaybackEnded;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ConversationEntry> _conversationEntries
            = new ObservableCollection<ConversationEntry>();

        public ObservableCollection<ConversationEntry> ConversationEntries
        {
            get => _conversationEntries;
        }

        public string ConversationAsText
        {
            get => RealtimeApiSdk.ConversationAsText;
        }

        public RealtimeApiWpfControl()
        {
            InitializeComponent();

            RealtimeApiSdk = new RealtimeApiSdk();
            this.VoiceVisualEffect = VoiceVisualEffect.SoundWave;

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

        public VoiceVisualEffect VoiceVisualEffect
        {
            get
            {
                VoiceVisualEffect rtn = VoiceVisualEffect.Cycle;
                switch (audioVisualizerView.VisualEffect)
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
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle;
                        break;
                    case VoiceVisualEffect.SoundWave:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumBar;
                        break;
                    case VoiceVisualEffect.Oscilloscope:
                        audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.Oscilloscope;
                        break;
                    case VoiceVisualEffect.Border:
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
            }
        }

        public void StopSpeechRecognition()
        {
            if (RealtimeApiSdk.IsRunning)
            {
                //Stop voice recognition;
                RealtimeApiSdk.StopSpeechRecognitionAsync();
                ReactToMicInput = false;
            }
        }

        public void RegisterFunctionCall(FunctionCallSetting functionCallSetting, Func<FuncationCallArgument, JObject> functionCallback)
        {
            RealtimeApiSdk.RegisterFunctionCall(functionCallSetting, functionCallback);
        }
        public void RefreshConversationData()
        {
            // Sync local ObservableCollection with the SDK's list
            SyncConversationEntriesFromSdk();

            NotifyPropertyChanged(nameof(ConversationEntries));
            NotifyPropertyChanged(nameof(ConversationAsText));
        }

        public void ClearConversationHistory()
        {
            RealtimeApiSdk.ClearConversationEntries();
            _conversationEntries.Clear();
            RefreshConversationData();
        }

        // Called whenever we want to refresh from the SDK
        private void SyncConversationEntriesFromSdk()
        {
            // This is the list in your SDK, e.g. a List<ConversationEntry>.
            var sdkEntries = RealtimeApiSdk.ConversationEntries;

            // 1) Remove any items in _conversationEntries that are no longer in the SDK
            for (int i = _conversationEntries.Count - 1; i >= 0; i--)
            {
                if (!sdkEntries.Contains(_conversationEntries[i]))
                {
                    _conversationEntries.RemoveAt(i);
                }
            }

            // 2) Add any new items from the SDK that we don't already have
            foreach (var sdkItem in sdkEntries)
            {
                if (!_conversationEntries.Contains(sdkItem))
                {
                    _conversationEntries.Add(sdkItem);
                }
            }
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

            // Raise event from sdk
            RealtimeApiSdk.SpeechStarted += (s, e) => { SpeechStarted?.Invoke(this, e); };
            RealtimeApiSdk.SpeechDataAvailable += RealtimeApiSdk_SpeechDataAvailable;
            RealtimeApiSdk.SpeechTextAvailable += (s, e) => { SpeechTextAvailable?.Invoke(this, e); };
            RealtimeApiSdk.SpeechEnded += (s, e) => { SpeechEnded?.Invoke(this, e); };

            RealtimeApiSdk.PlaybackStarted += (s, e) => { PlaybackStarted?.Invoke(this, e); };
            RealtimeApiSdk.PlaybackDataAvailable += RealtimeApiSdk_PlaybackDataAvailable;
            RealtimeApiSdk.PlaybackTextAvailable += (s, e) => { PlaybackTextAvailable?.Invoke(this, e); };
            RealtimeApiSdk.PlaybackEnded += (s, e) => { PlaybackEnded?.Invoke(this, e); };

            //audioVisualizerView.AudioSampleRate = speakerCapture.WaveFormat.SampleRate;
            audioVisualizerView.Scale = 5;
            audioVisualizerView.Start();
        }

        private void RealtimeApiSdk_SpeechDataAvailable(object? sender, AudioEventArgs e)
        {
            SpeechDataAvailable?.Invoke(this, e);

            byte[] iEEEAudioBytes = e.GetIEEEAudioBuffer();
            Audio_DataAvailable(null, new WaveInEventArgs(iEEEAudioBytes, iEEEAudioBytes.Length));

        }

        private void RealtimeApiSdk_PlaybackDataAvailable(object? sender, AudioEventArgs e)
        {
            PlaybackDataAvailable?.Invoke(this, e);

            byte[] iEEEAudioBytes = e.GetIEEEAudioBuffer();
            Audio_DataAvailable(null, new WaveInEventArgs(iEEEAudioBytes, iEEEAudioBytes.Length));
        }

        private void Audio_DataAvailable(object? sender, WaveInEventArgs e)
        {
            int length = e.BytesRecorded / 4;           // Float data
            double[] result = new double[length];

            for (int i = 0; i < length; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4);

            // Push into visualizer
            audioVisualizerView.PushSampleData(result);
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
 
    }
}
