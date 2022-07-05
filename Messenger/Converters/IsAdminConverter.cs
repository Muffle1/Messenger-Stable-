using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Messenger
{
    public class IsAdminConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
                return Visibility.Collapsed;
            if (values[0] is not Chat)
                return Visibility.Collapsed;
            if ((values[0] as Chat).UserChats == null)
                return Visibility.Collapsed;
            if (parameter.ToString() == "Options")
            {
                if (((values[0] as Chat).UserChats.Any(uc => uc.IsAdmin)) && ((values[0] as Chat)?.UserChats.SingleOrDefault(uc => uc.IsAdmin)?.Email == values[1].ToString()))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            if (parameter.ToString() == "Leave")
            {
                if (((values[0] as Chat).UserChats.Any(uc => uc.IsAdmin)) && ((values[0] as Chat)?.UserChats.SingleOrDefault(uc => uc.IsAdmin)?.Email != values[1].ToString()))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
