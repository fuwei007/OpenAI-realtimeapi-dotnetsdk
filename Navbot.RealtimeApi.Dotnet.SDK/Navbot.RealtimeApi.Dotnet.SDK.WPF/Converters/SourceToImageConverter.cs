using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

public class SourceToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string source)
        {
            if (source == "AI")
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/default-ai.png"));
            }
            if (source == "User")
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/default-user.png"));
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
