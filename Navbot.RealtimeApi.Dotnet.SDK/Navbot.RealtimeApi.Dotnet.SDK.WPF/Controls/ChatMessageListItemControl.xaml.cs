using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF
{
    /// <summary>
    /// Interaction logic for ChatMessageListItemControl.xaml
    /// </summary>
    public partial class ChatMessageListItemControl : UserControl
    {
        public ChatMessageListItemControl()
        {
            InitializeComponent();
        }

        #region DP: Font, Foreground, Background, Avatars

        public static readonly DependencyProperty ChatBubbleFontFamilyProperty =
            DependencyProperty.Register(nameof(ChatBubbleFontFamily),
                typeof(FontFamily),
                typeof(ChatMessageListItemControl),
                new PropertyMetadata(new FontFamily("Segoe UI")));

        public FontFamily ChatBubbleFontFamily
        {
            get => (FontFamily)GetValue(ChatBubbleFontFamilyProperty);
            set => SetValue(ChatBubbleFontFamilyProperty, value);
        }

        public static readonly DependencyProperty ChatBubbleForegroundProperty =
            DependencyProperty.Register(nameof(ChatBubbleForeground),
                typeof(Brush),
                typeof(ChatMessageListItemControl),
                new PropertyMetadata(Brushes.DarkBlue));

        public Brush ChatBubbleForeground
        {
            get => (Brush)GetValue(ChatBubbleForegroundProperty);
            set => SetValue(ChatBubbleForegroundProperty, value);
        }

        public static readonly DependencyProperty UserChatBubbleBackgroundProperty =
            DependencyProperty.Register(nameof(UserChatBubbleBackground),
                typeof(Brush),
                typeof(ChatMessageListItemControl),
                new PropertyMetadata(Brushes.LightGray));

        public Brush UserChatBubbleBackground
        {
            get => (Brush)GetValue(UserChatBubbleBackgroundProperty);
            set => SetValue(UserChatBubbleBackgroundProperty, value);
        }

        public static readonly DependencyProperty AiChatBubbleBackgroundProperty =
            DependencyProperty.Register(nameof(AiChatBubbleBackground),
                typeof(Brush),
                typeof(ChatMessageListItemControl),
                new PropertyMetadata(Brushes.LightGray));

        public Brush AiChatBubbleBackground
        {
            get => (Brush)GetValue(AiChatBubbleBackgroundProperty);
            set => SetValue(AiChatBubbleBackgroundProperty, value);
        }

        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register(nameof(UserAvatarSource),
                typeof(ImageSource),
                typeof(ChatMessageListItemControl),
                new PropertyMetadata(null));

        public ImageSource UserAvatarSource
        {
            get => (ImageSource)GetValue(UserAvatarSourceProperty);
            set => SetValue(UserAvatarSourceProperty, value);
        }

        public static readonly DependencyProperty AiAvatarSourceProperty =
            DependencyProperty.Register(nameof(AiAvatarSource),
                typeof(ImageSource),
                typeof(ChatMessageListItemControl),
                new PropertyMetadata(null));

        public ImageSource AiAvatarSource
        {
            get => (ImageSource)GetValue(AiAvatarSourceProperty);
            set => SetValue(AiAvatarSourceProperty, value);
        }

        #endregion
    }
}
