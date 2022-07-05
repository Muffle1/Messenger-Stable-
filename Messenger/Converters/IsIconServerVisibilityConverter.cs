using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Messenger
{
    public class IsIconServerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if ((parameter.ToString() == "TextBlock") && (string.IsNullOrEmpty((value as Server).FileName)))
                return Visibility.Visible;
            else if ((parameter.ToString() == "Border") && (!string.IsNullOrEmpty((value as Server).FileName)))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}