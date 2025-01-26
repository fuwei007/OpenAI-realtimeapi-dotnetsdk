using Navbot.RealtimeApi.Dotnet.SDK.Core.Enum;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

/// <summary>
/// Interaction logic for RealtimeApiControlFull.xaml
/// </summary>
public partial class RealtimeApiControlFull : UserControl
{
    public RealtimeApiControlFull()
    {
        InitializeComponent();
    }

    #region ShowChatTranscript  
    public static readonly DependencyProperty ShowChatTranscriptProperty =
        DependencyProperty.Register(
            nameof(ShowChatTranscript),
            typeof(bool),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(true));

    /// <summary>  
    /// Controls visibility of the conversation area on the right side.  
    /// </summary>  
    public bool ShowChatTranscript
    {
        get => (bool)GetValue(ShowChatTranscriptProperty);
        set => SetValue(ShowChatTranscriptProperty, value);
    }
    #endregion

    #region ShowButtonBar  
    public static readonly DependencyProperty ShowButtonBarProperty =
        DependencyProperty.Register(
            nameof(ShowButtonBar),
            typeof(bool),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(true));

    /// <summary>  
    /// Controls visibility of the bottom ButtonBarControl.  
    /// </summary>  
    public bool ShowButtonBar
    {
        get => (bool)GetValue(ShowButtonBarProperty);
        set => SetValue(ShowButtonBarProperty, value);
    }
    #endregion

    // -------------------------------------------------------------------  
    //  Pass-through properties for RealtimeApiWpfControl  
    // -------------------------------------------------------------------  

    #region OpenAiApiKey  

    public static readonly DependencyProperty OpenAiApiKeyProperty =
        DependencyProperty.Register(
            nameof(OpenAiApiKey),
            typeof(string),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(string.Empty, OnOpenAiApiKeyChanged));

    public string OpenAiApiKey
    {
        get => (string)GetValue(OpenAiApiKeyProperty);
        set => SetValue(OpenAiApiKeyProperty, value);
    }

    private static void OnOpenAiApiKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var parent = (RealtimeApiControlFull)d;
        if (parent.PART_RealtimeApi != null)
        {
            parent.PART_RealtimeApi.OpenAiApiKey = (string)e.NewValue;
        }
    }

    #endregion

    #region NetworkProtocolType  

    public static readonly DependencyProperty NetworkProtocolTypeProperty =
        DependencyProperty.Register(
            nameof(NetworkProtocolType),
            typeof(NetworkProtocolType),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(NetworkProtocolType.WebSocket, OnNetworkProtocolTypeChanged));

    public NetworkProtocolType NetworkProtocolType
    {
        get => (NetworkProtocolType)GetValue(NetworkProtocolTypeProperty);
        set => SetValue(NetworkProtocolTypeProperty, value);
    }

    private static void OnNetworkProtocolTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var parent = (RealtimeApiControlFull)d;
        if (parent.PART_RealtimeApi != null && e.NewValue is NetworkProtocolType nt)
        {
            parent.PART_RealtimeApi.NetworkProtocolType = nt;
        }
    }

    #endregion

    #region VoiceVisualEffect  

    public static readonly DependencyProperty VoiceVisualEffectProperty =
        DependencyProperty.Register(
            nameof(VoiceVisualEffect),
            typeof(VoiceVisualEffect),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(VoiceVisualEffect.SoundWave, OnVoiceVisualEffectChanged));

    public VoiceVisualEffect VoiceVisualEffect
    {
        get => (VoiceVisualEffect)GetValue(VoiceVisualEffectProperty);
        set => SetValue(VoiceVisualEffectProperty, value);
    }

    private static void OnVoiceVisualEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var parent = (RealtimeApiControlFull)d;
        if (parent.PART_RealtimeApi != null && e.NewValue is VoiceVisualEffect effect)
        {
            parent.PART_RealtimeApi.VoiceVisualEffect = effect;
        }
    }

    #endregion

    #region ReactToMicInput  

    public static readonly DependencyProperty ReactToMicInputProperty =
        DependencyProperty.Register(
            nameof(ReactToMicInput),
            typeof(bool),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(false, OnReactToMicInputChanged));

    public bool ReactToMicInput
    {
        get => (bool)GetValue(ReactToMicInputProperty);
        set => SetValue(ReactToMicInputProperty, value);
    }

    private static void OnReactToMicInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var parent = (RealtimeApiControlFull)d;
        if (parent.PART_RealtimeApi != null)
        {
            parent.PART_RealtimeApi.ReactToMicInput = (bool)e.NewValue;
        }
    }

    #endregion

    // -------------------------------------------------------------------  
    //  User-facing style properties for the ConversationControl  
    // -------------------------------------------------------------------  

    #region ChatBackground  
    public static readonly DependencyProperty ChatBackgroundProperty =
        DependencyProperty.Register(
            nameof(ChatBackground),
            typeof(Brush),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(null));

    /// <summary>  
    /// Background for the entire chat area (ConversationControl).  
    /// </summary>  
    public Brush ChatBackground
    {
        get => (Brush)GetValue(ChatBackgroundProperty);
        set => SetValue(ChatBackgroundProperty, value);
    }
    #endregion

    #region ChatBubbleForeground  
    public static readonly DependencyProperty ChatBubbleForegroundProperty =
        DependencyProperty.Register(
            nameof(ChatBubbleForeground),
            typeof(Brush),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(null));

    /// <summary>  
    /// The brush color for text inside the chat bubbles.  
    /// </summary>  
    public Brush ChatBubbleForeground
    {
        get => (Brush)GetValue(ChatBubbleForegroundProperty);
        set => SetValue(ChatBubbleForegroundProperty, value);
    }
    #endregion

    #region AiChatBubbleBackground  
    public static readonly DependencyProperty AiChatBubbleBackgroundProperty =
        DependencyProperty.Register(
            nameof(AiChatBubbleBackground),
            typeof(Brush),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(null));

    /// <summary>  
    /// Background color for AI chat bubbles.  
    /// </summary>  
    public Brush AiChatBubbleBackground
    {
        get => (Brush)GetValue(AiChatBubbleBackgroundProperty);
        set => SetValue(AiChatBubbleBackgroundProperty, value);
    }
    #endregion

    #region UserChatBubbleBackground  
    public static readonly DependencyProperty UserChatBubbleBackgroundProperty =
        DependencyProperty.Register(
            nameof(UserChatBubbleBackground),
            typeof(Brush),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(null));

    /// <summary>  
    /// Background color for the user's chat bubbles.  
    /// </summary>  
    public Brush UserChatBubbleBackground
    {
        get => (Brush)GetValue(UserChatBubbleBackgroundProperty);
        set => SetValue(UserChatBubbleBackgroundProperty, value);
    }
    #endregion

    #region ChatBubbleFontFamily  
    public static readonly DependencyProperty ChatBubbleFontFamilyProperty =
        DependencyProperty.Register(
            nameof(ChatBubbleFontFamily),
            typeof(FontFamily),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(new FontFamily("Segoe UI")));

    /// <summary>  
    /// Font family for both user and AI chat text.  
    /// </summary>  
    public FontFamily ChatBubbleFontFamily
    {
        get => (FontFamily)GetValue(ChatBubbleFontFamilyProperty);
        set => SetValue(ChatBubbleFontFamilyProperty, value);
    }
    #endregion

    #region AiAvatarSource  
    public static readonly DependencyProperty AiAvatarSourceProperty =
        DependencyProperty.Register(
            nameof(AiAvatarSource),
            typeof(ImageSource),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(null));

    /// <summary>  
    /// The avatar image for AI messages.  
    /// </summary>  
    public ImageSource AiAvatarSource
    {
        get => (ImageSource)GetValue(AiAvatarSourceProperty);
        set => SetValue(AiAvatarSourceProperty, value);
    }
    #endregion

    #region UserAvatarSource  
    public static readonly DependencyProperty UserAvatarSourceProperty =
        DependencyProperty.Register(
            nameof(UserAvatarSource),
            typeof(ImageSource),
            typeof(RealtimeApiControlFull),
            new PropertyMetadata(null));

    /// <summary>  
    /// The avatar image for user messages.  
    /// </summary>  
    public ImageSource UserAvatarSource
    {
        get => (ImageSource)GetValue(UserAvatarSourceProperty);
        set => SetValue(UserAvatarSourceProperty, value);
    }
    #endregion
}


