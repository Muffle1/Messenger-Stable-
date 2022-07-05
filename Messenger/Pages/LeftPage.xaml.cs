using System.Windows;
using System.Windows.Controls;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatPage.xaml
    /// </summary>
    public partial class LeftPage : UserControl
    {
        public LeftPage()
        {
            InitializeComponent();
        }

        private void Server_Click(object sender, RoutedEventArgs e)
        {
            Servers.SelectedItem = ((ListBoxItem)Servers.ContainerFromElement((Button)sender)).Content;
        }
    }
}
