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

public partial class ConversationControl : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty UserImageProperty = DependencyProperty.Register(
        nameof(UserImage), typeof(ImageSource), typeof(ConversationControl), new PropertyMetadata(null));

    public static readonly DependencyProperty AIImageProperty = DependencyProperty.Register(
        nameof(AIImage), typeof(ImageSource), typeof(ConversationControl), new PropertyMetadata(null));


    public static readonly DependencyProperty ConversationEntriesProperty = DependencyProperty.Register(
        nameof(ConversationEntries),
        typeof(ObservableCollection<ConversationEntry>),
        typeof(ConversationControl),
        new PropertyMetadata(null, OnConversationEntriesChanged));

    //public static readonly DependencyProperty SourceImageMappingProperty = DependencyProperty.Register(
    //    nameof(SourceImageMapping),
    //    typeof(Dictionary<string, ImageSource>),
    //    typeof(ConversationControl),
    //    new PropertyMetadata(null));

    public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
        nameof(FontSize), typeof(double), typeof(ConversationControl), new PropertyMetadata(14.0));

    public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
        nameof(FontFamily), typeof(FontFamily), typeof(ConversationControl), new PropertyMetadata(new FontFamily("Segoe UI")));

    public ObservableCollection<ConversationEntry> ConversationEntries
    {
        get => (ObservableCollection<ConversationEntry>)GetValue(ConversationEntriesProperty);
        set => SetValue(ConversationEntriesProperty, value);
    }

    public ImageSource UserImage
    {
        get => (ImageSource)GetValue(UserImageProperty);
        set => SetValue(UserImageProperty, value);
    }

    public ImageSource AIImage
    {
        get => (ImageSource)GetValue(AIImageProperty);
        set => SetValue(AIImageProperty, value);
    }

    //public Dictionary<string, ImageSource> SourceImageMapping
    //{
    //    get => (Dictionary<string, ImageSource>)GetValue(SourceImageMappingProperty);
    //    set => SetValue(SourceImageMappingProperty, value);
    //}

    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public new FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private static void OnConversationEntriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConversationControl control && e.NewValue is ObservableCollection<ConversationEntry> newEntries)
        {
            control.ScrollToBottom();
        }
    }

    public ConversationControl()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            ConversationEntries = new ObservableCollection<ConversationEntry>
            {
                new ConversationEntry { UTCTimestamp = DateTime.UtcNow, Source = "User", Content = "Hello, AI!" },
                new ConversationEntry { UTCTimestamp = DateTime.UtcNow, Source = "AI", Content = "Hello, User!" },
                new ConversationEntry { UTCTimestamp = DateTime.UtcNow, Source = "User", Content = "How are you today?" }
            };

            UserImage = new BitmapImage(new Uri("pack://application:,,,/Resources/default-user.png"));
            AIImage = new BitmapImage(new Uri("pack://application:,,,/Resources/default-ai.png"));
        }
    }

    private void ScrollToBottom()
    {
        if (VisualTreeHelper.GetChild(this, 0) is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToEnd();
        }
    }
}