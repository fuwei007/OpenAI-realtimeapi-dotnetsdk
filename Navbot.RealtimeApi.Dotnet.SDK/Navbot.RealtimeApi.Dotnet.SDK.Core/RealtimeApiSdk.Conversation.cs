using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using System.ComponentModel;
using System.Text;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core;

public partial class RealtimeApiSdk : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly List<ConversationEntry> conversationEntries = new List<ConversationEntry>();
    private readonly StringBuilder conversationTextBuilder = new StringBuilder();
    public List<ConversationEntry> ConversationEntries => conversationEntries;
    public string ConversationAsText => conversationTextBuilder.ToString();

    public void ClearConversationEntries()
    {
        conversationEntries.Clear();
        conversationTextBuilder.Clear();
    }

    private void AddConversationEntry(string source, string content)
    {
        if (IsContentEffectivelyEmpty(content))
        {
            return;
        }

        var entry = new ConversationEntry
        {
            UTCTimestamp = DateTime.UtcNow,
            Source = source,
            Content = content
        };

        conversationEntries.Add(entry);
        conversationTextBuilder.AppendLine($"{entry.UTCTimestamp:HH:mm:ss} [{entry.Source}] {entry.Content}");

        NotifyConversationAsTextChanged();
    }

    private bool IsContentEffectivelyEmpty(string content)
    {
        // Remove invisible characters for the check
        string tempContent = content.Replace("\r", "")
                                    .Replace("\n", "")
                                    .Replace("\t", "")
                                    .Replace("\u00A0", "")
                                    .Replace("\u200B", "")
                                    .Replace("\u200C", "")
                                    .Replace("\u200D", "")
                                    .Replace("\uFEFF", "");

        return string.IsNullOrWhiteSpace(tempContent);
    }

    protected virtual void OnSpeechTextAvailable(TranscriptEventArgs e)
    {
        AddConversationEntry("user", e.Transcript.TrimEnd());
        SpeechTextAvailable?.Invoke(this, e);
    }

    protected virtual void OnPlaybackTextAvailable(TranscriptEventArgs e)
    {
        AddConversationEntry("ai", e.Transcript.TrimEnd());
        PlaybackTextAvailable?.Invoke(this, e);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void NotifyConversationAsTextChanged()
    {
        OnPropertyChanged(nameof(ConversationAsText));
    }
}
