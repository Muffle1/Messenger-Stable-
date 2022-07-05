using System.ComponentModel;

namespace Messenger
{
    public enum ViewType
    {
        [Description("Всплывающий элемент")]
        Popup,
        [Description("Страница")]
        Page,
        [Description("Менеджер")]
        Manager
    }
}