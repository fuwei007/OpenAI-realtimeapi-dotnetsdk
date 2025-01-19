using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

/// <summary>
/// A converter that takes the Source of the message (User or AI) 
/// and returns HorizontalAlignment.Left for "ai" or HorizontalAlignment.Right for "user".
/// </summary>
public class SourceAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string source)
        {
            if (source.ToLowerInvariant() == "ai")
            {
                return HorizontalAlignment.Left;
            }
            else if (source.ToLowerInvariant() == "user")
            {
                return HorizontalAlignment.Right;
            }
        }
            return HorizontalAlignment.Left;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
