using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF;

public class SourceAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string source)
        {
            var alignmentParam = parameter as string;
            if (alignmentParam == "Left")
                return source == "User" ? true : false;

            if (alignmentParam == "Right")
                return source == "AI" ? true : false;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
