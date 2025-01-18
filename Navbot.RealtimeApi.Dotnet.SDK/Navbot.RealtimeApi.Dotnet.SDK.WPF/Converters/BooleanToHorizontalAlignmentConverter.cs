using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

public class BooleanToHorizontalAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isLeftAligned)
        {
            return isLeftAligned ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }
        return HorizontalAlignment.Left;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}