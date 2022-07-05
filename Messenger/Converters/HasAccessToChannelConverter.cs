using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Messenger
{
    public class HasAccessToChannelConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return false;

            return ((values[0] as Chat).Roles.Any(r => r.Id_Role == (values[1] as Role)?.Id_Role));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
