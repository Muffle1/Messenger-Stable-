using System.IO;
using System.Windows;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ServerContext.Server = new StreamReader("serverconfig.txt").ReadToEnd();
        }
    }
}