using System;
using System.Globalization;
using System.Windows.Data;

namespace Messenger
{
    public class IsRoleEditableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return false;

            if (((string)value == "Admin") || ((string)value == "Everyone"))
                return false;

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
