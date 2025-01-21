using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        DependencyProperty.Register(
            nameof(ConversationEntries),
            typeof(ObservableCollection<ConversationEntry>),
            typeof(ConversationControl),
            new PropertyMetadata(null, OnConversationEntriesChanged));

    public ObservableCollection<ConversationEntry> ConversationEntries
    {
        get => (ObservableCollection<ConversationEntry>)GetValue(ConversationEntriesProperty);
        set => SetValue(ConversationEntriesProperty, value);
    }

    private static void OnConversationEntriesChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (ConversationControl)d;

        // Unhook old collection
        if (e.OldValue is ObservableCollection<ConversationEntry> oldCollection)
        {
            oldCollection.CollectionChanged -= control.ConversationEntries_CollectionChanged;
        }

        // Hook new collection
        if (e.NewValue is ObservableCollection<ConversationEntry> newCollection)
        {
            newCollection.CollectionChanged += control.ConversationEntries_CollectionChanged;
        }
    }

    private void ConversationEntries_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // When new messages are added, scroll to the bottom.
        if (e.Action == NotifyCollectionChangedAction.Add
            && e.NewItems != null
            && e.NewItems.Count > 0)
        {
            // We do a dispatch to wait until layout is updated
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Access the ListBox in XAML
                if (ConversationListBox != null && ConversationListBox.Items.Count > 0)
                {
                    // Scroll to the last item
                    ConversationListBox.ScrollIntoView(
                        ConversationListBox.Items[ConversationListBox.Items.Count - 1]);
                }
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }

    #endregion

    #region Styling/Appearance Properties

    public static readonly DependencyProperty ChatBackgroundProperty =
        DependencyProperty.Register(
            nameof(ChatBackground),
            typeof(Brush),
            typeof(ConversationControl),
            new PropertyMetadata(Brushes.White));

    public Brush ChatBackground
    {
        get => (Brush)GetValue(ChatBackgroundProperty);
        set => SetValue(ChatBackgroundProperty, value);
    }

    // Chat bubble font (for both user and AI, unless you split them further)
    public static readonly DependencyProperty ChatBubbleFontFamilyProperty =
        DependencyProperty.Register(
            nameof(ChatBubbleFontFamily),
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