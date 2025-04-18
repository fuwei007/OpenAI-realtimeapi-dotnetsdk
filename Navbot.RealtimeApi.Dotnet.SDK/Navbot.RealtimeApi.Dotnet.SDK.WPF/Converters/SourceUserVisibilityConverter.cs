﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

/// <summary>
/// A converter that takes the Source of the message (User or AI) And if AI, returns visible and if User, returns Hidden/collapsed
/// </summary>
public class SourceUserVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string source)
        {
            if (source.ToLowerInvariant() == "ai")
            {
                return Visibility.Hidden;
            }
            else if (source.ToLowerInvariant() == "user")
            {
                return Visibility.Visible;
            }
        }
            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
