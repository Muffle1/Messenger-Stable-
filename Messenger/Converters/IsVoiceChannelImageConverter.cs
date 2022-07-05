using System;
using System.Globalization;
using System.Windows.Data;

namespace Messenger
{
    public class IsVoiceChannelImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "/Images/voice.png";
            return "/Images/note.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
