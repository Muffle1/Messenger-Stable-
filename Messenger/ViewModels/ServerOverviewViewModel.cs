using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

namespace Messenger
{
    public class ServerOverviewViewModel : BaseViewModel
    {
        #region Комманды
        public RelayCommand CopyInviteCodeCommand { get; set; }
        public RelayCommand SaveServerChangedCommand { get; set; }
        public RelayCommand ChangeIconServerCommand { get; set; }
        #endregion

        private readonly Server _server;

        public Server ServerChange { get; set; }

        public ImageSource _serverIcon;
        public ImageSource ServerIcon
        {
            get => _serverIcon;

            set
            {
                _serverIcon = value;
                OnPropertyChanged(nameof(ServerIcon));
            }
        }

        public ServerOverviewViewModel(Server server)
        {
            _server = server;
            ServerChange = new Server()
            {
                Id_Server = _server.Id_Server,
                Name = _server.Name,
                File = _server.File,
                FileName = _server.FileName,
                InviteCode = _server.InviteCode
            };
            ServerIcon = ServerChange.BitmapFile;
            CopyInviteCodeCommand = new RelayCommand(CopyInviteCode);
            ChangeIconServerCommand = new RelayCommand(UploadPhoto);
            SaveServerChangedCommand = new RelayCommand(SaveServerChanged);
        }

        public override void SubscribeEvent()
        {
            ServerContext.ChangeServerCmdReceived += OnChangeServerCmdReceived;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.ChangeServerCmdReceived -= OnChangeServerCmdReceived;
        }

        private void OnChangeServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Server server && server.Id_Server == _server.Id_Server)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ServerChange.Name = server.Name;
                    ServerChange.File = server.File;
                    ServerChange.FileName = server.FileName;
                    ServerIcon = ServerChange.BitmapFile;
                });
            }
        }

        private void SaveServerChanged(object obj)
        {
            Package package = new(Command.ChangeServer, ServerChange);
            ServerContext.SendRequest(package);
        }


        private void UploadPhoto(object obj)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image|*.jpg;*.png;";
            if (openFileDialog.ShowDialog() == true)
            {
                ServerChange = FileHelper.OpenFile(ServerChange, openFileDialog.FileName) as Server;
                if (ServerChange.File != null)
                {
                    ServerIcon = FileHelper.GetBitmapFromByteArray(ServerChange.File);
                    OnPropertyChanged(nameof(ServerIcon));
                }
            }
        }

        public void CopyInviteCode(object obj) =>
            Clipboard.SetText(_server.InviteCode);
    }
}
