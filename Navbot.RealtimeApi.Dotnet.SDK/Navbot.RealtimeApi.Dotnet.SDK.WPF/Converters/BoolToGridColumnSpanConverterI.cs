using System.Globalization;
using System.Windows.Data;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

public class BoolToGridColumnSpanConverterI : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool showChatTranscript)
        {
            return showChatTranscript ? 1 : 3; // Span all columns if ChatTranscript is hidden
        }
        return 1; // Default
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
