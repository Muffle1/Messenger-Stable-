using Microsoft.Win32;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Messenger
{
    public class AddServerViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand CreateServerCommand { get; set; }
        public RelayCommand UploadPhotoCommand { get; set; }
        public RelayCommand JoinServerCommand { get; set; }
        #endregion

        public Server Server { get; set; } = new();
        public ImageSource ServerIcon { get; set; } = new BitmapImage(new Uri($"{Environment.CurrentDirectory}\\Images\\camera.jpg"));
        public string ServerCode { get; set; }

        public AddServerViewModel()
        {
            CreateServerCommand = new RelayCommand(CreateServer);
            UploadPhotoCommand = new RelayCommand(UploadPhoto);
            JoinServerCommand = new RelayCommand(JoinServer);
        }

        private void CreateServer(object obj)
        {
            if (!string.IsNullOrEmpty(Server.Name))
            {
                Package package = new(Command.CreateServer, Server);
                ServerContext.SendRequest(package);

                OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "AddServerViewModel");
                return;
            }

            Server.AddError(nameof(Server.Name), "Не верное имя!");
        }

        private void UploadPhoto(object obj)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image|*.jpg;*.png;";
            if (openFileDialog.ShowDialog() == true)
            {
                Server = FileHelper.OpenFile(Server, openFileDialog.FileName) as Server;

                if (Server.File != null)
                {
                    ServerIcon = FileHelper.GetBitmapFromByteArray(Server.File);
                    OnPropertyChanged(nameof(ServerIcon));
                }
            }
        }

        private void JoinServer(object obj)
        {
            if (!string.IsNullOrEmpty(ServerCode))
            {
                Package package = new(Command.JoinServer, ServerCode);
                ServerContext.SendRequest(package);
                OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "AddServerViewModel");
                return;
            }
        }
    }
}
