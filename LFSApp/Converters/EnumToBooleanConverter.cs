using System;
using System.Globalization;
using System.Windows;

namespace LFSApp.Converters
{
    public class EnumToBooleanConverter : BaseValueConverter<EnumToBooleanConverter>
    {
        //<RadioButton IsChecked="{Binding Path=EnumName, Converter={StaticResource enumBooleanConverter}, ConverterParameter=enumItem}">Text</RadioButton>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (!Enum.IsDefined(value.GetType(), value))
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }
}