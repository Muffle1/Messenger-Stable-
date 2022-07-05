using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.Json;
using System.Windows;

namespace Messenger
{
    public static class NotifyHelper
    {
        /// <summary>
        /// Уведомление об приходе сообщения
        /// </summary>
        /// <param name="message">Сообщение</param>
        public static void Notify(Message message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new ToastContentBuilder()
                    .AddArgument("message", "message")
                    .AddText($"Сообщение от {message.FromEmail}")
                    .AddText($"{message.Body}")
                    .Show();
            });
        }

        /// <summary>
        /// Уведомление о приходе заявки в друзья
        /// </summary>
        /// <param name="request">заявка</param>
        public static void Notify(Request request)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                new ToastContentBuilder()
                    .AddArgument("friendEmail", JsonSerializer.Serialize(request))
                    .AddText("Заявка в друзья")
                    .AddText($"{request.Sender} хочет добавить вас в друзья")
                    .AddButton(new ToastButton()
                        .SetContent("Принять")
                        .AddArgument("action", "accept")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("Отклонить")
                        .AddArgument("action", "decline")
                        .SetBackgroundActivation())
                    .Show();
            });
        }
    }
}