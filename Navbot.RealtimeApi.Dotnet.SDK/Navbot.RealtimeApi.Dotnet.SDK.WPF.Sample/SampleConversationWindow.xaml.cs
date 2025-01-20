using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;


namespace Navbot.RealtimeApi.Dotnet.SDK.WPF.Sample
{
    /// <summary>
    /// Interaction logic for SampleConversationWindow.xaml
    /// </summary>
    public partial class SampleConversationWindow : Window
    {
        private ObservableCollection<ConversationEntry> _conversation;

        public SampleConversationWindow()
        {
            InitializeComponent();

            // Initialize the conversation with some messages
            _conversation = new ObservableCollection<ConversationEntry>
            {
                new ConversationEntry { Source="user", Content="Hello, AI!", },
                new ConversationEntry { Source="ai",   Content="Hello, user!", },
            };

            ConversationControl.ConversationEntries = _conversation;
        }

        private void OnAddAiMessage(object sender, RoutedEventArgs e)
        {
            _conversation.Add(new ConversationEntry
            {
                Source = "ai",
                Content = "This is a new AI message",
                UTCTimestamp = System.DateTime.UtcNow
            });
        }

        private void OnAddUserMessage(object sender, RoutedEventArgs e)
        {
            _conversation.Add(new ConversationEntry
            {
                Source = "user",
                Content = "This is a new User message",
                UTCTimestamp = System.DateTime.UtcNow
            });
        }
    }
}
