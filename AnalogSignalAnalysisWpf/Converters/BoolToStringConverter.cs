using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnalogSignalAnalysisWpf.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool status = (bool)value;

            if (status == true)
            {
                return "断开";
            }
            else
            {
                return "连接";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
