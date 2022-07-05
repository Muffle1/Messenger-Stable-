using System;
using System.Globalization;
using System.Windows.Data;

namespace Messenger
{
    public class ChannelTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (((bool)value) && (parameter.ToString() == "VoiceChat"))
                return true;
            else if ((!(bool)value) && (parameter.ToString() == "TextChat"))
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((bool)value) && (parameter.ToString() == "VoiceChat"))
                return true;
            else if ((!(bool)value) && (parameter.ToString() == "TextChat"))
                return true;

            return false;
        }
    }
}