using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KitchenEquipmentDemo.Enterprise.WPF.Converters
{
    /// <summary>
    /// If value (current selected enum) equals parameter (target enum),
    /// returns the "NavButtonSelected" style; otherwise returns "NavButton".
    /// Styles must be defined in App resources (merged Controls.xaml).
    /// </summary>
    public sealed class NavStyleConverter : IValueConverter
    {
        private const string SelectedKey = "NavButtonSelected";
        private const string UnselectedKey = "NavButton";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSelected = AreEqualEnum(value, parameter);
            var key = isSelected ? SelectedKey : UnselectedKey;

            var style = System.Windows.Application.Current?.TryFindResource(key) as Style;
            return style ?? DependencyProperty.UnsetValue; // avoid exceptions if missing
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static bool AreEqualEnum(object value, object parameter)
        {
            if (value == null || parameter == null) return false;

            var valueType = value.GetType();
            if (valueType.IsEnum)
            {
                // parameter from x:Static will already be the same enum type
                if (parameter.GetType() == valueType)
                    return value.Equals(parameter);

                // if parameter is string, try parse
                if (parameter is string s && Enum.IsDefined(valueType, s))
                    return value.Equals(Enum.Parse(valueType, s, ignoreCase: false));
            }
            return value.Equals(parameter);
        }
    }
}
