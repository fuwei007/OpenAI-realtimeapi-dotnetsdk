using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Navbot.RealtimeApi.Dotnet.SDK.Core; // For RealtimeApiSdk
using Navbot.RealtimeApi.Dotnet.SDK.WPF; // For RealtimeApiWpfControl

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

public partial class ButtonBarControl : UserControl
{
    private static readonly ILog log = LogManager.GetLogger(typeof(ButtonBarControl));

    // Keeps track of whether we are currently recording (start/stop speech recognition).  
    private bool isRecording = false;
    // Keeps track of whether the mic is muted (for press-to-talk).  
    private bool isMuted = true;
    // Delay for re-muting after mouse-up.  
    private int millisecondsDelay = 1000;
    private CancellationTokenSource muteDelayCancellationTokenSource;

    public ButtonBarControl()
    {
        InitializeComponent();
    }

    #region RealtimeControl Dependency Property  
    /// <summary>  
    /// A DependencyProperty so that this control can bind to the RealtimeApiWpfControl  
    /// in MainWindow (or wherever else you place it).  
    /// </summary>  
    public static readonly DependencyProperty RealtimeControlProperty =
        DependencyProperty.Register(
            nameof(RealtimeControl),
            typeof(RealtimeApiWpfControl),
            typeof(ButtonBarControl),
            new PropertyMetadata(null));

    /// <summary>  
    /// The RealtimeApiWpfControl we will control (start/stop speech recognition, etc.).  
    /// </summary>  
    public RealtimeApiWpfControl RealtimeControl
    {
        get => (RealtimeApiWpfControl)GetValue(RealtimeControlProperty);
        set => SetValue(RealtimeControlProperty, value);
    }
    #endregion

    /// <summary>  
    /// Handles the click for the Play/Pause button to start/stop speech recognition.  
    /// </summary>  
    private void btnStartStopRecognition_Click(object sender, RoutedEventArgs e)
    {
        // Get references to the icons in the ControlTemplate to swap them on click  
        var playIcon = PlayPauseButton.Template.FindName("PlayIcon", PlayPauseButton) as Path;
        var pauseIcon = PlayPauseButton.Template.FindName("PauseIcon", PlayPauseButton) as Path;
        if (playIcon == null || pauseIcon == null)
            return;

        if (isRecording)
        {
            // Switch to "Play" icon  
            playIcon.Visibility = Visibility.Visible;
            pauseIcon.Visibility = Visibility.Collapsed;

            // Stop speech recognition  
            RealtimeControl?.StopSpeechRecognition();
        }
        else
        {
            // Switch to "Pause" icon  
            playIcon.Visibility = Visibility.Collapsed;
            pauseIcon.Visibility = Visibility.Visible;

            // Start speech recognition  
            RealtimeControl?.StartSpeechRecognition();
            // Immediately ensure we are muted = false, or handle how you want  
            // but in the original code, after "Start", we do "DisableTalkingMode()"  
            DisableTalkingMode();
        }

        isRecording = !isRecording;
    }

    /// <summary>  
    /// Press-to-talk: when user holds mouse down, unmute mic; on release, re-mute after short delay.  
    /// </summary>  
    private void PressToTalkButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        EnableTalkingMode();
    }

    private void PressToTalkButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        DisableTalkingModeWithDelay();
    }

    /// <summary>  
    /// After user releases press-to-talk button, re-mute mic after a short delay.  
    /// Cancels any previous re-mute tasks if the user quickly presses again.  
    /// </summary>  
    private async void DisableTalkingModeWithDelay()
    {
        // Cancel any previous unmute->mute transition  
        muteDelayCancellationTokenSource?.Cancel();
        muteDelayCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(millisecondsDelay, muteDelayCancellationTokenSource.Token);
            DisableTalkingMode();
        }
        catch (TaskCanceledException)
        {
            // If the task was canceled, do nothing  
        }
    }

    private void EnableTalkingMode()
    {
        // Cancel any pending re-mute  
        muteDelayCancellationTokenSource?.Cancel();

        if (PressToTalkButton.Template.FindName("MuteCrossIcon", PressToTalkButton) is Path muteCrossIcon)
        {
            // Unmute  
            isMuted = false;
            muteCrossIcon.Visibility = Visibility.Collapsed;
        }

        if (RealtimeControl != null)
        {
            RealtimeControl.RealtimeApiSdk.IsMuted = isMuted;
            RealtimeControl.ReactToMicInput = true;
        }

        log.Info("Microphone unmuted (press-to-talk active).");
    }

    private void DisableTalkingMode()
    {
        if (PressToTalkButton.Template.FindName("MuteCrossIcon", PressToTalkButton) is Path muteCrossIcon)
        {
            // Mute  
            isMuted = true;
            muteCrossIcon.Visibility = Visibility.Visible;
        }

        if (RealtimeControl != null)
        {
            RealtimeControl.RealtimeApiSdk.IsMuted = isMuted;
            RealtimeControl.ReactToMicInput = false;
        }

        log.Info("Microphone muted.");
    }
}