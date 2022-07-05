using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для VoiceChatPage.xaml
    /// </summary>
    public partial class VoiceChatPage : UserControl
    {
        public VoiceChatPage()
        {
            InitializeComponent();
        }

        private void OnServerClick(object sender, RoutedEventArgs e)
        {
            UsersInVoiceChatList.SelectedItem = ((ListBoxItem)UsersInVoiceChatList.ContainerFromElement((Button)sender)).Content;
        }

        private void OnMicroClick(object sender, RoutedEventArgs e)
        {
            if (MicroImage.Tag.ToString() == "MicroOn")
            {
                MicroImage.Source = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\Images\\MicroOff.png"));
                MicroImage.Tag = "MicroOff";
                return;
            }

            MicroImage.Source = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\Images\\MicroOn.png"));
            MicroImage.Tag = "MicroOn";
        }
    }
}
