using System.Windows;
using System.Windows.Controls;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для MyAccountPage.xaml
    /// </summary>
    public partial class MyAccountPage : UserControl
    {
        public MyAccountPage()
        {
            InitializeComponent();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (EditBtn.Content.ToString() == "Готово")
                NameBox.IsEnabled = false;
            else
                NameBox.IsEnabled = true;
        }
    }
}
