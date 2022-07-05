using System.Linq;
using System.Threading;
using System.Windows;

namespace Messenger
{
    public class LeftViewModel : BaseViewModel
    {
        #region Комманды
        public RelayCommand OpenCommand { get; private set; }
        public RelayCommand PopupCommand { get; private set; }
        public RelayCommand ExitCommand { get; private set; }

        #endregion

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }

        private Server _selectedServer;
        public Server SelectedServer
        {
            get => _selectedServer;

            set
            {
                if (SelectedServer != value)
                {
                    _selectedServer = value;
                    if (SelectedServer != null)
                    {
                        OnViewSwitched(new ServerChannelListViewModel(SelectedServer.Id_Server, CurrentUser), ViewType.Page, ViewPlace.Middle);
                        OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(FriendsViewModel));
                    }
                    OnPropertyChanged(nameof(SelectedServer));
                }
            }
        }

        private readonly CancellationTokenSource _tokenSource;

        public LeftViewModel(User user, CancellationTokenSource tokenSource)
        {
            OpenCommand = new RelayCommand(OpenPage);
            CurrentUser = user;
            _tokenSource = tokenSource;
            PopupCommand = new RelayCommand(Popup);
            ExitCommand = new RelayCommand(Exit);
        }

        public override void SubscribeEvent()
        {
            ServerContext.CreateServerCmdReceived += OnCreateServerCmdReceived;
            ServerContext.ChangeServerCmdReceived += OnChangeServerCmdReceived;
            ServerContext.RemoveRoleCmdReceived += OnRemoveRoleCmdReceived;
            ServerContext.DeleteServerCmdReceived += OnDeleteServerCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived += OnRemoveUserFromServerCmdReceived;
            ServerContext.ExitCmdReceived += OnExitCmdReceived;
        }

        private void OnExitCmdReceived()
        {
            OnViewSwitched(new AuthenticationViewModel(), ViewType.Manager);
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.CreateServerCmdReceived -= OnCreateServerCmdReceived;
            ServerContext.ChangeServerCmdReceived -= OnChangeServerCmdReceived;
            ServerContext.RemoveRoleCmdReceived -= OnRemoveRoleCmdReceived;
            ServerContext.DeleteServerCmdReceived -= OnDeleteServerCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived -= OnRemoveUserFromServerCmdReceived;
        }

        private void OnCreateServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Server server)
                Application.Current.Dispatcher.Invoke(() => CurrentUser.Servers.Add(server));
        }

        private void OnRemoveRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is int id)
                CurrentUser.Roles.Remove(CurrentUser.Roles.FirstOrDefault(r => r.Id_Role == id));
        }

        private void OnChangeServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Server server)
            {
                Server oldServer = CurrentUser.Servers.FirstOrDefault(s => s.Id_Server == server.Id_Server);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    oldServer.Name = server.Name;
                    oldServer.File = server.File;
                    oldServer.FileName = server.FileName;
                });
            }
        }

        private void OnDeleteServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is int idServer)
                CloseServer(idServer);
        }

        private void OnRemoveUserFromServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is ServerObject serverObject) && ((string)serverObject.Parameter == CurrentUser.Email))
                CloseServer(serverObject.IdServer);
        }

        private void CloseServer(int idServer)
        {
            if (SelectedServer?.Id_Server == idServer)
            {
                OnViewSwitched(null, ViewType.Page, ViewPlace.Middle, nameof(ServerChannelListViewModel));
                OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(ChatServerViewModel));
                OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(ServerManagementViewModel));
                OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(ServerAddRoleToUserViewModel));
            }

            Application.Current.Dispatcher.Invoke(() => 
                CurrentUser.Servers.Remove(CurrentUser.Servers.FirstOrDefault(s => s.Id_Server == idServer)));
        }

        private void OpenPage(object obj)
        {
            switch ((string)obj)
            {
                case "Добавить сервер":
                    OnViewSwitched(new AddServerViewModel(), ViewType.Popup);
                    break;
                case "Мой профиль":
                    OnViewSwitched(new MyAccountViewModel(CurrentUser), ViewType.Popup);
                    break;
                case "Главная":
                    {
                        SelectedServer = null;
                        OnViewSwitched(null, ViewType.Page, ViewPlace.Right);
                        OnViewSwitched(new ChatsListViewModel(CurrentUser), ViewType.Page, ViewPlace.Middle);
                        OnViewSwitched(new FriendsViewModel(CurrentUser), ViewType.Page, ViewPlace.Right);
                    }
                    break;
            }
        }

        private void Popup(object obj)
        {
            switch ((string)obj)
            {
                case "MyProfile":
                    OnViewSwitched(new MyAccountViewModel(CurrentUser), ViewType.Popup);
                    break;
                case "Settings":
                    OnViewSwitched(new SettingsViewModel(CurrentUser), ViewType.Popup);
                    break;
                default:
                    //OnViewSwitched(new AccountViewModel(CurrentUser.Friends.SingleOrDefault(x => x.Email == (string)obj)), ViewType.Popup);
                    break;
            }
        }

        private void Exit(object obj)
        {
            new ConfigurationUserHelper().Write(new User());
            OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(VoiceChatViewModel));
            Package package = new(Command.Exit, "Exit");
            ServerContext.SendRequest(package);
            _tokenSource.Cancel();
        }
    }
}