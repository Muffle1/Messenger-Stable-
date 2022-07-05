using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for ServerManagementPage.xaml
    /// </summary>
    public partial class ServerManagementPage : UserControl
    {
        public ServerManagementPage()
        {
            InitializeComponent();
            overviewButton.Background = new SolidColorBrush(Color.FromRgb(57, 60, 67));
            overviewButton.Foreground = new SolidColorBrush(Colors.White);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            overviewButton.Background = new SolidColorBrush(Colors.Transparent);
            roleButton.Background = new SolidColorBrush(Colors.Transparent);

            (sender as Button).Background = new SolidColorBrush(Color.FromRgb(57, 60, 67));
            (sender as Button).Foreground = new SolidColorBrush(Colors.White);
        }
    }
}
