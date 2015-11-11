using System;
using System.Globalization;
using System.Windows.Data;
using static System.Boolean;

namespace Core
{
    public class InverseBooleanConverter : IValueConverter
    {
        private static InverseBooleanConverter _inverseBooleanConverter;
        public static InverseBooleanConverter Default { get { return _inverseBooleanConverter = (_inverseBooleanConverter ?? new InverseBooleanConverter()); } }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;
            if (value == null || !TryParse(value.ToString(), out result))
            {
                return default(bool);
            }

            return !result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
