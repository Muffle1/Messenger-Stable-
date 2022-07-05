using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Messenger
{
    public class IsDialogConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Visibility.Hidden;
            //Сравнение по количеству участников добавлено в целях оптимизации (думаю чекнуть по нему будет быстрее чем всех челов по циклу оббежать)
            if ((value is Chat) && ((value as Chat).UserChats.Count <= 2) && (value as Chat).Server_Id == null && !(value as Chat).UserChats.Any(c => c.IsAdmin == true))
                return Visibility.Visible;
            else return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
