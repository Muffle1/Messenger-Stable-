using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Messenger
{
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызов события при смене значения в свойстве
        /// </summary>
        /// <param name="prop">Свойство</param>
        public void OnPropertyChanged([CallerMemberName] string prop = "") =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
