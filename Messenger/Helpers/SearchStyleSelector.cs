using System.Windows;
using System.Windows.Controls;

namespace Messenger
{
    public class SearchStyleSelector : StyleSelector
    {
        public Style ChatClassStyle { get; set; }
        public Style UserClassStyle { get; set; }

        /// <summary>
        /// Выбор стиля
        /// </summary>
        /// <param name="item">Объект элемента</param>
        /// <param name="container">Контейнер</param>
        /// <returns></returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is User)
                return UserClassStyle;
            else if (item is Chat)
                return ChatClassStyle;
            else return null;
        }
    }
}
