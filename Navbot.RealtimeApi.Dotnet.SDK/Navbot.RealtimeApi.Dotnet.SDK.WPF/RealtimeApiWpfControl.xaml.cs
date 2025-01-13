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
        private const string apiKey = "";
        private VisualEffect voiceVisualEffect;

        //TODO2 Move into Api Sdk
        private WaveInEvent speechWaveIn;

        // TODO2
        private WasapiLoopbackCapture speakerCapture;
        private BufferedWaveProvider speakerWaveProvider;

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
            Loaded += RealtimeApiWpfControl_Loaded;
            RealtimeApiSdk.SpeechTextAvailable += OnConversationUpdated;
            RealtimeApiSdk.PlaybackTextAvailable += OnConversationUpdated;
        }

        public RealtimeApiSdk RealtimeApiSdk { get; private set; }
        public string OpenAiApiKey
        {
            get { return RealtimeApiSdk.ApiKey; }
            set { RealtimeApiSdk.ApiKey = value; }
        }

        public VisualEffect VoiceVisualEffect
        {
            get { return voiceVisualEffect; }
            set { voiceVisualEffect = value; }
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
            // TODO2 connect to sdk event
            

            //RealtimeApiSdk.WaveInDataAvailable += RealtimeApiSdk_WaveInDataAvailable;
            //RealtimeApiSdk.WaveInDataAvailable += SpeechWaveIn_DataAvailable;

            speakerCapture = new WasapiLoopbackCapture();
            speakerWaveProvider = new BufferedWaveProvider(speakerCapture.WaveFormat)
            {
                BufferLength = 1024 * 1024, // 1 MB buffer (adjust based on your needs)
                DiscardOnBufferOverflow = true // Optional: discard data when buffer is full}
            };

            speakerCapture.DataAvailable += SpeakerCapture_DataAvailable;
            RealtimeApiSdk.WebSocketResponse += RealtimeApiSdk_WebSocketResponse;

            RealtimeApiSdk.SpeechStarted += RealtimeApiSdk_SpeechStarted;
            RealtimeApiSdk.SpeechDataAvailable += RealtimeApiSdk_SpeechDataAvailable;
            RealtimeApiSdk.SpeechTextAvailable += RealtimeApiSdk_SpeechTextAvailable;
            RealtimeApiSdk.SpeechEnded += RealtimeApiSdk_SpeechEnded;

            RealtimeApiSdk.PlaybackStarted += RealtimeApiSdk_PlaybackStarted;
            RealtimeApiSdk.PlaybackDataAvailable += RealtimeApiSdk_PlaybackDataAvailable;
            RealtimeApiSdk.PlaybackTextAvailable += RealtimeApiSdk_PlaybackTextAvailable;
            RealtimeApiSdk.PlaybackEnded += RealtimeApiSdk_PlaybackEnded;

            voiceVisualEffect = VoiceVisualEffect;

            audioVisualizerView.AudioSampleRate = speakerCapture.WaveFormat.SampleRate;
            audioVisualizerView.Scale = 5;
            audioVisualizerView.VisualEffect = AudioVisualizer.Core.Enum.VisualEffect.SpectrumCycle;

            audioVisualizerView.StartRenderAsync();
            
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

        private void RealtimeApiSdk_WaveInDataAvailable(object? sender, WaveInEventArgs e)
        {
            OnWaveInDataAvailable(e);
        }

        protected virtual void OnWaveInDataAvailable(WaveInEventArgs e)
        {
            WaveInDataAvailable?.Invoke(this, e);
        }

        private void RealtimeApiSdk_WebSocketResponse(object? sender, WebSocketResponseEventArgs e)
        {
            OnWebSocketResponse(e);
        }

        protected virtual void OnWebSocketResponse(WebSocketResponseEventArgs e)
        {
            WebSocketResponse?.Invoke(this, e);
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
