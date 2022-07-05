using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatListPage.xaml
    /// </summary>
    public partial class ChatsListPage : UserControl
    {
        public ChatsListPage()
        {
            InitializeComponent();
            friendsOpenButton.Foreground = new SolidColorBrush(Colors.White);
            friendsOpenButton.Background = new SolidColorBrush(Color.FromRgb(52, 55, 60));
        }

        private void friendsOpenButton_Click(object sender, RoutedEventArgs e)
        {
            ChatList.SelectedIndex = -1;
            (sender as Button).Foreground = new SolidColorBrush(Colors.White);
            (sender as Button).Background = new SolidColorBrush(Color.FromRgb(52, 55, 60));
        }

        private void ChatList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatList.SelectedIndex != -1)
            {
                friendsOpenButton.Foreground = new SolidColorBrush(Color.FromRgb(142, 146, 151));
                friendsOpenButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
