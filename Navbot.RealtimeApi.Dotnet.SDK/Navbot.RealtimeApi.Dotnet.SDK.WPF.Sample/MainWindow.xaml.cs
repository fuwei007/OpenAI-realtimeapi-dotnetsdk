using log4net;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Enum;
using System.Drawing;
using System.Windows.Media;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainWindow));
        private bool isRecording = false;
        private bool isMuted = true;
        private bool _showButtonPanel;
        private bool _showChatTranscript;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.ShowButtonPanel = true;
            this.ShowChatTranscript = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";

            realtimeApiWpfControl.OpenAiApiKey = openAiApiKey;
            realtimeApiWpfControl.NetworkProtocolType = NetworkProtocolType.WebSocket;
            realtimeApiWpfControl.VoiceVisualEffect = VoiceVisualEffect.SoundWave;

            realtimeApiWpfControl.RealtimeApiSdk.PropertyChanged += RealtimeApiSdk_PropertyChanged;

            // Register FunctionCall for weather
            realtimeApiWpfControl.RegisterFunctionCall(new FunctionCallSetting
            {
                Name = "get_weather",
                Description = "Get current weather for a specified city",
                Parameter = new FunctionParameter
                {
                    Properties = new Dictionary<string, FunctionProperty>
                    {
                        {
                            "city", new FunctionProperty
                            {
                                Description = "The name of the city for which to fetch the weather."
                            }
                        }
                    },
                    Required = new List<string> { "city" }
                }
            }, FunctionCallHelper.HandleWeatherFunctionCall);

            // Register FunctionCall for run application
            realtimeApiWpfControl.RegisterFunctionCall(new FunctionCallSetting
            {
                Name = "write_notepad",
                Description = "Open a text editor and write the time, for example, 2024-10-29 16:19. Then, write the content, which should include my questions along with your answers.",
                Parameter = new FunctionParameter
                {
                    Properties = new Dictionary<string, FunctionProperty>
                    {
                        {
                            "content", new FunctionProperty
                            {
                                Description = "The content consists of my questions along with the answers you provide."
                            }
                        },
                        {
                            "date", new FunctionProperty
                            {
                                Description = "The time, for example, 2024-10-29 16:19."
                            }
                        }
                    },
                    Required = new List<string> { "content", "date" }
                }
            }, FunctionCallHelper.HandleNotepadFunctionCall);

            // Register FunctionCall for color

            #region Updtae Style
            realtimeApiWpfControl.RegisterFunctionCall(new FunctionCallSetting
            {
                Name = "changeControlPanelColor",
                Description = "Change the color of the control panel",
                Parameter = new FunctionParameter
                {
                    Properties = new Dictionary<string, FunctionProperty>
                    {
                        {
                            "color", new FunctionProperty
                            {
                                Description = "Get the color to be specified.(For example: #FFF, #000)"
                            }
                        }
                    },
                    Required = new List<string> { "color" }
                }
            }, FunctionCallHelper.ChangeControlPanelColor);

            realtimeApiWpfControl.RegisterFunctionCall(new FunctionCallSetting
            {
                Name = "changeChatBackgroundColor",
                Description = "Change the background color of the chat interface",
                Parameter = new FunctionParameter
                {
                    Properties = new Dictionary<string, FunctionProperty>
                    {
                        {
                            "color", new FunctionProperty
                            {
                                Description = "Get the color to be specified.(For example: #FFF, #000)"
                            }
                        },
                    },
                    Required = new List<string> { "color" }
                }
            }, FunctionCallHelper.ChangeChatBackgroundColor);

            realtimeApiWpfControl.RegisterFunctionCall(new FunctionCallSetting
            {
                Name = "changeFontColor",
                Description = "Change the color of the font",
                Parameter = new FunctionParameter
                {
                    Properties = new Dictionary<string, FunctionProperty>
                    {
                        {
                            "color", new FunctionProperty
                            {
                                Description = "Get the color to be specified.(For example: #FFF, #000)"
                            }
                        },
                        {
                            "size", new FunctionProperty
                            {
                                Description = "Get font size (For example:  10, 20)"
                            }
                        },
                    },
                    Required = new List<string> { "color" }
                }
            }, FunctionCallHelper.ChangeFontStyle);
            #endregion


            log.Info("App Start...");
        }

        private bool ShowButtonPanel
        {
            get => _showButtonPanel;
            set
            {
                if (_showButtonPanel != value)
                {
                    _showButtonPanel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool ShowChatTranscript
        {
            get => _showChatTranscript;
            set
            {
                if (_showChatTranscript != value)
                {
                    _showChatTranscript = value;
                    OnPropertyChanged();
                }
            }
        }

        private void RealtimeApiSdk_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(realtimeApiWpfControl.RealtimeApiSdk.ConversationAsText))
            //{
            //    scrollViewer.ScrollToEnd();
            //}
        }

        /// <summary>
        /// Start / Stop Speech Recognition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void btnStartStopRecognition_Click(object sender, RoutedEventArgs e)
        //{
        //    var playIcon = (System.Windows.Shapes.Path)PlayPauseButton.Template.FindName("PlayIcon", PlayPauseButton);
        //    var pauseIcon = (System.Windows.Shapes.Path)PlayPauseButton.Template.FindName("PauseIcon", PlayPauseButton);

        //    if (isRecording)
        //    {
        //        playIcon.Visibility = Visibility.Visible;
        //        pauseIcon.Visibility = Visibility.Collapsed;

        //        realtimeApiWpfControl.StopSpeechRecognition();
        //    }
        //    else
        //    {
        //        playIcon.Visibility = Visibility.Collapsed;
        //        pauseIcon.Visibility = Visibility.Visible;

        //        realtimeApiWpfControl.StartSpeechRecognition();

        //        // Disable talking mode by default
        //        DisableTalkingMode();
        //    }

        //    isRecording = !isRecording;
        //}

        #region INotifyPropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged

        #region Talking Mode
        //private void PressToTalkButton_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    EnableTalkingMode();
        //}

        //private void PressToTalkButton_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    DisableTalkingModeWithDelay();
        //}

        //private CancellationTokenSource muteDelayCancellationTokenSource;
        //private int millisecondsDelay = 1000;

        //private async void DisableTalkingModeWithDelay()
        //{
        //    // Cancel any previous mute delay if the button is pressed again quickly
        //    muteDelayCancellationTokenSource?.Cancel();
        //    muteDelayCancellationTokenSource = new CancellationTokenSource();

        //    try
        //    {
        //        // Introduce a delay before muting
        //        await Task.Delay(millisecondsDelay, muteDelayCancellationTokenSource.Token); // 500ms delay

        //        // Perform mute logic only if the task wasn't canceled
        //        DisableTalkingMode();
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        // Ignore if the delay was canceled
        //    }
        //}

        //private void EnableTalkingMode()
        //{
        //    muteDelayCancellationTokenSource?.Cancel(); // Cancel any pending mute
        //    var muteCrossIcon = (System.Windows.Shapes.Path)PressToTalkButton.Template.FindName("MuteCrossIcon", PressToTalkButton);
        //    isMuted = false;

        //    muteCrossIcon.Visibility = Visibility.Collapsed;

        //    // Unmute microphone
        //    realtimeApiWpfControl.RealtimeApiSdk.IsMuted = isMuted;
        //    realtimeApiWpfControl.ReactToMicInput = true;
        //    log.Info("Microphone unmuted");
        //}

        //private void DisableTalkingMode()
        //{
        //    var muteCrossIcon = (System.Windows.Shapes.Path)PressToTalkButton.Template.FindName("MuteCrossIcon", PressToTalkButton);
        //    isMuted = true;
        //    muteCrossIcon.Visibility = Visibility.Visible;

        //    // Mute microphone
        //    realtimeApiWpfControl.RealtimeApiSdk.IsMuted = isMuted;
        //    realtimeApiWpfControl.ReactToMicInput = false;
        //    log.Info("Microphone muted");
        //}

        #endregion Talking Mode
    }
}