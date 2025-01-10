using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using System.ComponentModel;

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
            WaveCanvas.SizeChanged += WaveCanvas_SizeChanged;
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
            speechWaveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 1)
            };

            speechWaveIn.DataAvailable += SpeechWaveIn_DataAvailable;

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
            PlayVisualVoiceEffect(false);

            DrawDefaultVisualEffect(voiceVisualEffect);
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

                // Start ripple effect.
                PlayVisualVoiceEffect(true);
            }
        }

        public void StopSpeechRecognition()
        {
            if (RealtimeApiSdk.IsRunning)
            {
                // Stop the ripple effect.
                PlayVisualVoiceEffect(false);
                ReactToMicInput = false;

                // Stop voice recognition;
                RealtimeApiSdk.StopSpeechRecognitionAsync();
            }
        }

        public void RegisterFunctionCall(FunctionCallSetting functionCallSetting, Func<FuncationCallArgument, JObject> functionCallback)
        {
            RealtimeApiSdk.RegisterFunctionCall(functionCallSetting, functionCallback);
        }

        private void PlayVisualVoiceEffect(bool enable)
        {
            WaveCanvas.Visibility = voiceVisualEffect == WPF.VisualEffect.SoundWave ? Visibility.Visible : Visibility.Collapsed;
            //RippleEffect.Visibility = voiceVisualEffect == WPF.VisualEffect.Cycle ? Visibility.Visible : Visibility.Collapsed;
            RippleEffect.Visibility = Visibility.Collapsed;
            cycleWaveformCanvas.Visibility = voiceVisualEffect == WPF.VisualEffect.Cycle ? Visibility.Visible : Visibility.Collapsed;
            ReactToMicInput = enable;

            switch (voiceVisualEffect)
            {
                case WPF.VisualEffect.Cycle:
                    HandleVoiceEffect(enable);
                    WaveCanvas.Children.Clear();
                    break;
                case WPF.VisualEffect.SoundWave:
                    HandleVoiceEffect(enable);
                    cycleWaveformCanvas.Children.Clear();
                    break;
                default:
                    break;
            }
        }

        private void HandleVoiceEffect(bool enable)
        {
            if (enable)
            {
                speakerCapture.StartRecording();
                speechWaveIn.StartRecording();

            }
            else
            {
                speakerCapture.StopRecording();
                speechWaveIn.StopRecording();
            }
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

            try
            {
                switch (this.voiceVisualEffect)
                {
                    case WPF.VisualEffect.Cycle:
                        Dispatcher.Invoke(() => cycleWaveformCanvas.UpdateAudioData(audioBuffer.ToArray()));
                        break;
                    case WPF.VisualEffect.SoundWave:
                        Dispatcher.Invoke(() => DrawWaveform(audioBuffer.ToArray()));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex) { }
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

            try
            {
                switch (this.voiceVisualEffect)
                {
                    case WPF.VisualEffect.Cycle:
                        Dispatcher.Invoke((Delegate)(() => cycleWaveformCanvas.UpdateAudioData(audioBuffer)));
                        break;
                    case WPF.VisualEffect.SoundWave:
                        Dispatcher.Invoke((Delegate)(() => DrawWaveform(audioBuffer)));
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex) { }
        }

        private void RealtimeApiSdk_PlaybackAudioReceived(object? sender, AudioEventArgs e)
        {
            try
            {
                switch (this.voiceVisualEffect)
                {
                    case WPF.VisualEffect.Cycle:
                        Dispatcher.Invoke(() => cycleWaveformCanvas.UpdateAudioData(e.GetWaveBuffer()));
                        break;
                    case WPF.VisualEffect.SoundWave:
                        Dispatcher.Invoke(() => DrawWaveform(e.GetWaveBuffer()));
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex) { }
        }

        //private void DrawWaveform(float[] waveform)
        //{
        //    WaveCanvas.Children.Clear();

        //    double canvasWidth = WaveCanvas.ActualWidth;
        //    double canvasHeight = WaveCanvas.ActualHeight;

        //    if (canvasWidth == 0 || canvasHeight == 0 || waveform == null)
        //        return;

        //    Polyline polyline = new Polyline
        //    {
        //        Stroke = Brushes.LimeGreen,
        //        StrokeThickness = 2
        //    };

        //    double step = canvasWidth / waveform.Length;
        //    double centerY = canvasHeight / 2;

        //    for (int i = 0; i < waveform.Length; i++)
        //    {
        //        double x = i * step;
        //        double y = centerY - (waveform[i] * centerY);
        //        polyline.Points.Add(new Point(x, y));
        //    }

        //    WaveCanvas.Children.Add(polyline);
        //}


        #region DrawCoolWaveform
        private void DrawCoolWaveform(float[] waveform)
        {
            WaveCanvas.Children.Clear();

            double canvasWidth = WaveCanvas.ActualWidth;
            double canvasHeight = WaveCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0 || waveform == null)
                return;

            WaveCanvas.Background = CreateDynamicGradient();

            Polyline positiveWave = CreateWaveformPolyline(Colors.Cyan, canvasWidth, canvasHeight, waveform, 1);
            Polyline negativeWave = CreateWaveformPolyline(Colors.Magenta, canvasWidth, canvasHeight, waveform, -1);

            WaveCanvas.Children.Add(positiveWave);
            WaveCanvas.Children.Add(negativeWave);

            AddParticleEffects(waveform, canvasWidth, canvasHeight);
        }

        private LinearGradientBrush CreateDynamicGradient()
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradientBrush.GradientStops.Add(new GradientStop(Colors.DarkBlue, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Purple, 0.5));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.OrangeRed, 1.0));

            DoubleAnimation gradientAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            gradientBrush.GradientStops[0].BeginAnimation(GradientStop.OffsetProperty, gradientAnimation);

            return gradientBrush;
        }

        private Polyline CreateWaveformPolyline(Color color, double canvasWidth, double canvasHeight, float[] waveform, int direction)
        {
            Polyline polyline = new Polyline
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 3,
                Opacity = 0.8
            };

            DropShadowEffect glowEffect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.8
            };
            polyline.Effect = glowEffect;

            double step = canvasWidth / waveform.Length;
            double centerY = canvasHeight / 2;

            for (int i = 0; i < waveform.Length; i++)
            {
                double x = i * step;
                double y = centerY - (waveform[i] * centerY * direction); 
                polyline.Points.Add(new Point(x, y));
            }

            return polyline;
        }

        private void AddParticleEffects(float[] waveform, double canvasWidth, double canvasHeight)
        {
            Random random = new Random();

            for (int i = 0; i < waveform.Length / 10; i++) 
            {
                double x = random.NextDouble() * canvasWidth;
                double y = random.NextDouble() * canvasHeight;

                Ellipse particle = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = new SolidColorBrush(Color.FromRgb(
                        (byte)random.Next(50, 255),
                        (byte)random.Next(50, 255),
                        (byte)random.Next(50, 255))),
                    Opacity = 0.7
                };

                Canvas.SetLeft(particle, x);
                Canvas.SetTop(particle, y);
                WaveCanvas.Children.Add(particle);

                DoubleAnimation moveAnimation = new DoubleAnimation
                {
                    From = y,
                    To = y - 100, 
                    Duration = TimeSpan.FromSeconds(2),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                particle.BeginAnimation(Canvas.TopProperty, moveAnimation);
            }
        }
        #endregion

        #region DrawGradientLine
        private void DrawWaveform(float[] waveform)
        {
            WaveCanvas.Children.Clear();

            double canvasWidth = WaveCanvas.ActualWidth;
            double canvasHeight = WaveCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0 || waveform == null)
                return;

            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Purple, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));

            Polyline polyline = new Polyline
            {
                Stroke = gradientBrush,
                StrokeThickness = 2,
                Opacity = 0.8
            };

            DropShadowEffect shadowEffect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 5,
                ShadowDepth = 2,
                Opacity = 0.6
            };
            polyline.Effect = shadowEffect;

            double step = canvasWidth / waveform.Length;
            double centerY = canvasHeight / 2;

            for (int i = 0; i < waveform.Length; i++)
            {
                double x = i * step;
                double y = centerY - (waveform[i] * centerY);
                polyline.Points.Add(new Point(x, y));
            }

            WaveCanvas.Children.Add(polyline);
        }
       
        #endregion


        private void DrawDefaultVisualEffect(VisualEffect effect)
        {
            WaveCanvas.Children.Clear();

            switch (effect)
            {
                case WPF.VisualEffect.Cycle:
                    DrawCircle();
                    break;

                case WPF.VisualEffect.SoundWave:
                    DrawLine();
                    break;
                default:
                    break;
            }
        }

        private void DrawCircle(double sizeFactor = 0.95)
        {
            double canvasWidth = cycleWaveformCanvas.ActualWidth;
            double canvasHeight = cycleWaveformCanvas.ActualHeight;
            if (canvasWidth == 0 || canvasHeight == 0)
                return;

            double radius = Math.Min(canvasWidth, canvasHeight) * sizeFactor / 2;
            double centerX = canvasWidth / 2;
            double centerY = canvasHeight / 2;

            Ellipse circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.LimeGreen,
                StrokeThickness = 2
            };

            Canvas.SetLeft(circle, centerX - radius);
            Canvas.SetTop(circle, centerY - radius);

            cycleWaveformCanvas.Children.Clear();

            cycleWaveformCanvas.Children.Add(circle);
        }

        private void DrawLine()
        {
            double canvasWidth = WaveCanvas.ActualWidth;
            double canvasHeight = WaveCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
                return;


            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Purple, 0.0));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));

            Line line = new Line
            {
                X1 = 10,
                Y1 = canvasHeight / 2,
                X2 = canvasWidth - 10,
                Y2 = canvasHeight / 2,
                Stroke = gradientBrush,
                StrokeThickness = 2
            };


            WaveCanvas.Children.Add(line);
        }

        private void WaveCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawDefaultVisualEffect(voiceVisualEffect);
        }

        private void cycleWaveformCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawDefaultVisualEffect(voiceVisualEffect);
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
