using System.Globalization;
using System.Windows.Data;

namespace AISRouting.App.WPF.Converters
{
    /// <summary>
    /// Converts a count to boolean (true if greater than 0, false otherwise).
    /// </summary>
    public class CountToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
