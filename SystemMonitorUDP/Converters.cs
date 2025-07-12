using System.Globalization;
using System.Windows.Data;

namespace SystemMonitorUDP
{
    public static class Converters
    {
        public static readonly IValueConverter BoolInverter = new BoolInverterConverter();
    }

    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }
}