using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

public partial class ConversationControl : UserControl
{
    #region ConversationEntries
    public static readonly DependencyProperty ConversationEntriesProperty =
        DependencyProperty.Register(nameof(ConversationEntries),
            typeof(IEnumerable<ConversationEntry>),
            typeof(ConversationControl),
            new PropertyMetadata(null));

    public IEnumerable<ConversationEntry> ConversationEntries
    {
        get => (IEnumerable<ConversationEntry>)GetValue(ConversationEntriesProperty);
        set => SetValue(ConversationEntriesProperty, value);
    }
    #endregion

    #region Styling/Appearance Properties

    // Chat bubble font (for both user and AI, unless you split them further)
    public static readonly DependencyProperty ChatBubbleFontFamilyProperty =
        DependencyProperty.Register(nameof(ChatBubbleFontFamily),
            typeof(FontFamily),
            typeof(ConversationControl),
            new PropertyMetadata(new FontFamily("Segoe UI")));

    public FontFamily ChatBubbleFontFamily
    {
        get => (FontFamily)GetValue(ChatBubbleFontFamilyProperty);
        set => SetValue(ChatBubbleFontFamilyProperty, value);
    }

    // Chat bubble foreground color
    public static readonly DependencyProperty ChatBubbleForegroundProperty =
        DependencyProperty.Register(nameof(ChatBubbleForeground),
            typeof(Brush),
            typeof(ConversationControl),
            new PropertyMetadata(Brushes.DarkBlue));

    public Brush ChatBubbleForeground
    {
        get => (Brush)GetValue(ChatBubbleForegroundProperty);
        set => SetValue(ChatBubbleForegroundProperty, value);
    }

    // Background color for User chat bubble
    public static readonly DependencyProperty UserChatBubbleBackgroundProperty =
        DependencyProperty.Register(nameof(UserChatBubbleBackground),
            typeof(Brush),
            typeof(ConversationControl),
            new PropertyMetadata(Brushes.LightGray));

    public Brush UserChatBubbleBackground
    {
        get => (Brush)GetValue(UserChatBubbleBackgroundProperty);
        set => SetValue(UserChatBubbleBackgroundProperty, value);
    }

    // Background color for AI chat bubble
    public static readonly DependencyProperty AiChatBubbleBackgroundProperty =
        DependencyProperty.Register(nameof(AiChatBubbleBackground),
            typeof(Brush),
            typeof(ConversationControl),
            new PropertyMetadata(Brushes.LightGray));

    public Brush AiChatBubbleBackground
    {
        get => (Brush)GetValue(AiChatBubbleBackgroundProperty);
        set => SetValue(AiChatBubbleBackgroundProperty, value);
    }

    // User avatar image
    public static readonly DependencyProperty UserAvatarSourceProperty =
        DependencyProperty.Register(nameof(UserAvatarSource),
            typeof(ImageSource),
            typeof(ConversationControl),
            new PropertyMetadata(null));

    public ImageSource UserAvatarSource
    {
        get => (ImageSource)GetValue(UserAvatarSourceProperty);
        set => SetValue(UserAvatarSourceProperty, value);
    }

    // AI avatar image
    public static readonly DependencyProperty AiAvatarSourceProperty =
        DependencyProperty.Register(nameof(AiAvatarSource),
            typeof(ImageSource),
            typeof(ConversationControl),
            new PropertyMetadata(null));

    public ImageSource AiAvatarSource
    {
        get => (ImageSource)GetValue(AiAvatarSourceProperty);
        set => SetValue(AiAvatarSourceProperty, value);
    }

    #endregion

    public ConversationControl()
    {
        InitializeComponent();
    }
}