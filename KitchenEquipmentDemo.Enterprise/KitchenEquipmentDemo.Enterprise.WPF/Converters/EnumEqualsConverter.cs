using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KitchenEquipmentDemo.Enterprise.WPF.Converters
{
    /// <summary>
    /// Returns style resource based on enum comparison
    /// </summary>
    public class EnumToStyleConverter : IValueConverter
    {
        public string NormalStyleKey { get; set; }
        public string SelectedStyleKey { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return System.Windows.Application.Current.TryFindResource(NormalStyleKey);

            var enumType = value.GetType();
            if (!enumType.IsEnum)
                return System.Windows.Application.Current.TryFindResource(NormalStyleKey);

            var paramValue = CoerceParam(enumType, parameter);
            if (paramValue == null)
                return System.Windows.Application.Current.TryFindResource(NormalStyleKey);

            var styleKey = value.Equals(paramValue) ? SelectedStyleKey : NormalStyleKey;
            return System.Windows.Application.Current.TryFindResource(styleKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static object CoerceParam(Type enumType, object parameter)
        {
            // Same implementation as before
            if (parameter == null) return null;

            if (parameter.GetType() == enumType) return parameter;

            if (parameter is string s)
            {
                if (Enum.IsDefined(enumType, s))
                    return Enum.Parse(enumType, s, ignoreCase: false);

                foreach (var name in Enum.GetNames(enumType))
                {
                    if (string.Equals(name, s, StringComparison.OrdinalIgnoreCase))
                        return Enum.Parse(enumType, name);
                }
                return null;
            }

            try
            {
                var underlying = System.Convert.ChangeType(parameter, Enum.GetUnderlyingType(enumType), CultureInfo.InvariantCulture);
                return Enum.ToObject(enumType, underlying);
            }
            catch
            {
                return null;
            }
        }
    }
}