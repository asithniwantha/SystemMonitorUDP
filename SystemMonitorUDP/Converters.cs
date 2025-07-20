using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SystemMonitorUDP
{
    public static class Converters
    {
        public static readonly IValueConverter BoolInverter = new BoolInverterConverter();
        public static readonly IValueConverter HostStatusToColor = new HostStatusToColorConverter();
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

    public class HostStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "ready" => new SolidColorBrush(Colors.White),
                    "starting..." => new SolidColorBrush(Colors.Yellow),
                    "active" => new SolidColorBrush(Colors.LightGreen),
                    "resolved" => new SolidColorBrush(Colors.LightGreen),
                    "stopped" => new SolidColorBrush(Colors.Gray),
                    "restarting..." => new SolidColorBrush(Colors.Orange),
                    var s when s.Contains("failed") => new SolidColorBrush(Colors.Orange),
                    var s when s.Contains("error") => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}