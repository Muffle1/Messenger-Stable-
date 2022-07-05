using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Messenger
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IViewSwitcher, IEventSubscriber
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ViewHandler SwitchView;

        /// <summary>
        /// Измененние значения свойства
        /// </summary>
        /// <param name="prop">Свойство</param>
        public void OnPropertyChanged([CallerMemberName] string prop = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        /// <summary>
        /// Переклчатель страниц
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="viewType">Имя view</param>
        public void OnViewSwitched(IViewSwitcher view, ViewType viewType, ViewPlace viewPlace = ViewPlace.Default, string viewNameToClose = null) =>
            SwitchView?.Invoke(this, new ViewEventArgs(view, viewType, viewPlace, viewNameToClose));

        public virtual void SubscribeEvent()
        {

        }

        public virtual void UnsubscribeFromEvent()
        {

        }
    }
}
