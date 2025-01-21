using System;
using System.Globalization;
using System.Windows.Data;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

public class ProportionalWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double actualWidth && double.TryParse(parameter?.ToString(), out double proportion))
        {
            return actualWidth * proportion;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
