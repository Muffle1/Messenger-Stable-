using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Messenger
{
    /// <summary>
    /// Логика взаимодействия для ServerChannelList.xaml
    /// </summary>
    public partial class ServerChannelList : UserControl
    {
        public ServerChannelList()
        {
            InitializeComponent();
        }

        private void ChatChannel_Click(object sender, RoutedEventArgs e) =>
            TextChannels.SelectedItem = ((ListBoxItem)TextChannels.ContainerFromElement((Button)sender)).Content;

        private void VoiceChannel_Click(object sender, RoutedEventArgs e) =>
            VoiceChannels.SelectedItem = ((ListBoxItem)VoiceChannels.ContainerFromElement((Button)sender)).Content;

        private void TextChannels_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
