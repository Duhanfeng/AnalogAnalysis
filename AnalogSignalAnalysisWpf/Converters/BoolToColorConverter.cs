using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnalogSignalAnalysisWpf.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool status = (bool)value;

            if (status == true)
            {
                return Brushes.Green;
            }
            else
            {
                return Brushes.DarkGray;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
