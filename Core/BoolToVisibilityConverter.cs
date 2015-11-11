using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Core
{
    /// <summary>
    /// Converter from <see cref="bool"/> value to <see cref="Visibility"/>.
    /// </summary>
    /// <date>15.03 12.04.2010</date>
    /// <author>Anton Liakhovich</author>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        private readonly bool _isInverted;

        private static BoolToVisibilityConverter _boolToVisibilityConverter;
        public static BoolToVisibilityConverter Default { get { return _boolToVisibilityConverter = (_boolToVisibilityConverter ?? new BoolToVisibilityConverter(false)); } }

        private static BoolToVisibilityConverter _invertedBoolToVisibilityConverter;
        public static BoolToVisibilityConverter Invert { get { return _invertedBoolToVisibilityConverter = (_invertedBoolToVisibilityConverter ?? new BoolToVisibilityConverter(true)); } }

        public BoolToVisibilityConverter(bool isInverted)
        {
            _isInverted = isInverted;
        }

        #region Implementation of IValueConverter

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the target dependency property.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                return default(Visibility);
            }

            var result = (bool)value;

            return result ?
                (_isInverted ? Visibility.Collapsed : Visibility.Visible) :
                (_isInverted ? Visibility.Visible : Visibility.Collapsed);
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the source object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility))
            {
                return default(bool);
            }

            var result = (Visibility)value;

            return result == Visibility.Visible
                ? (!_isInverted)
                : (_isInverted);
        }

        #endregion
    }

}
