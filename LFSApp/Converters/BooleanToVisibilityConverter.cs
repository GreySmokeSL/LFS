using System;
using System.Globalization;
using System.Windows;

namespace LFSApp.Converters
{
    /// <summary>
    /// A converter that takes in a boolean and returns a <see cref="Visibility"/>
    /// </summary>
    public class BooleanToVisiblityConverter : BaseValueConverter<BooleanToVisiblityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hide = string.IsNullOrWhiteSpace(parameter?.ToString());
            return (bool?)value??false ? Visibility.Visible : hide ? Visibility.Hidden : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
