using System;
using System.Globalization;
using System.Windows.Data;
namespace Messenger
{
    public class DateConverter : IValueConverter
    {
        /// <summary>
        /// Перевод входного значения от переменной привязки в объект типа DateTime, который будет передан приемнику привязки
        /// </summary>
        /// <param name="value">входное значение</param>
        /// <param name="targetType">тип объекта, к которому привязан конвертер</param>
        /// <param name="parameter">параметры конвертера</param>
        /// <param name="culture">культура языка</param>
        /// <returns>объект типа DateTime</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToDateTime(value) == new DateTime())
                return new DateTime();
            if (value != null)
            {
                if ((string)parameter == "Year")
                    return System.Convert.ToDateTime(value).Year;
                if ((string)parameter == "Month")
                    return (Month)System.Convert.ToDateTime(value).Month;
                if ((string)parameter == "Day")
                    return System.Convert.ToDateTime(value).Day;
            }
            else
                return new DateTime();
            return new DateTime();
        }

        /// <summary>
        /// Перевод входного значения от приемника привязки в объект типа DateTime, который будет передан переменной привязки
        /// </summary>
        /// <param name="value">входное значение</param>
        /// <param name="targetType">тип объекта, к которому привязан конвертер</param>
        /// <param name="parameter">параметры конвертера</param>
        /// <param name="culture">культура языка</param>
        /// <returns>объект типа DateTime</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if ((string)parameter == "Year")
                    return new DateTime((int)value, 1, 1, 1, 0, 0);
                if ((string)parameter == "Month")
                    return new DateTime(1, (int)value, 1, 0, 1, 0);
                if ((string)parameter == "Day")
                    return new DateTime(1, 1, (int)value, 0, 0, 1);
            }
            else
                return new DateTime();
            return new DateTime();
        }
    }
}
