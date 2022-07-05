using System.Windows;
using System.Windows.Controls;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для RolePage.xaml
    /// </summary>
    public partial class ServerRolePage : UserControl
    {
        public ServerRolePage()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox).SelectedIndex != -1)
            {
                string roleName = ((sender as ListBox).SelectedItem as Role).Name;

                Access.Visibility = Visibility.Visible;
                Show.Visibility = Visibility.Visible;
                Menu.SelectedIndex = 0;

                if (roleName == "Admin")
                {
                    Access.Visibility = Visibility.Collapsed;
                    Show.Visibility = Visibility.Collapsed;
                    Menu.SelectedIndex = 2;
                    return;
                }
                else if (roleName == "Everyone")
                {
                    Show.Visibility = Visibility.Collapsed;
                    Menu.SelectedIndex = 1;
                    return;
                }
            }
        }
    }
}
