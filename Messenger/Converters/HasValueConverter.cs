using System;
using System.Globalization;
using System.Windows.Data;

namespace Messenger
{
    public class HasValueConverter : IValueConverter
    {
        /// <summary>
        /// Перевод входного значения от переменной привязки в объект типа DateTime, который будет передан приемнику привязки
        /// </summary>
        /// <param name="value">входное значение</param>
        /// <param name="targetType">тип объекта, к которому привязан конвертер</param>
        /// <param name="parameter">параметры конвертера</param>
        /// <param name="culture">культура языка</param>
        /// <returns>объект типа DateTime</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (string)value != "";

        /// <summary>
        /// Вывод пустого исключения
        /// </summary>
        /// <param name="value">входное значение</param>
        /// <param name="targetType">тип объекта, к которому привязан конвертер</param>
        /// <param name="parameter">параметры конвертера</param>
        /// <param name="culture">культура языка</param>
        /// <returns>объект типа DateTime</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
