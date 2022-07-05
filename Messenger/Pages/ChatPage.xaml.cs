using System.Windows.Controls;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatPage.xaml
    /// </summary>
    public partial class ChatPage : UserControl
    {
        public ChatPage()
        {
            InitializeComponent();
            ChatViewModel.ScrollToBottom += ChatViewModel_ScrollToBottom;
        }

        private void ChatViewModel_ScrollToBottom()
        {
            if (Messages.Items.Count > 0)
                Messages.ScrollIntoView(Messages.Items[^1]);
        }
    }
}
