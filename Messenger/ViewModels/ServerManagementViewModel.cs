namespace Messenger
{
    public class ServerManagementViewModel : BaseViewModel
    {
        public RelayCommand SwitchTabCommand { get; private set; }
        public RelayCommand DeleteServerCommand { get; private set; }

        private IViewSwitcher _tabPage;
        public IViewSwitcher TabPage
        {
            get => _tabPage;
            set
            {
                if (_tabPage != null)
                    (_tabPage as IEventSubscriber).UnsubscribeFromEvent();

                _tabPage = value;
                if (_tabPage != null)
                    (_tabPage as IEventSubscriber).SubscribeEvent();

                OnPropertyChanged(nameof(TabPage));
            }
        }

        private readonly Server _server;
        private readonly User _currentUser;

        public ServerManagementViewModel(Server server, User currentUser)
        {
            _server = server;
            _currentUser = currentUser;
            SwitchTabCommand = new RelayCommand(SwitchTab);
            DeleteServerCommand = new RelayCommand(DeleteServer);
            LoadPage(new ServerOverviewViewModel(_server));
        }

        private void SwitchTab(object obj)
        {
            switch ((string)obj)
            {
                case "Overview":
                    LoadPage(new ServerOverviewViewModel(_server));
                    break;
                case "Role":
                    LoadPage(new ServerRoleViewModel(_server, _currentUser));
                    break;
                case "Exit":
                    OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "ServerManagementViewModel");
                    break;
            }
        }

        private void DeleteServer(object obj)
        {
            Package package = new(Command.DeleteServer, _server.Id_Server);
            ServerContext.SendRequest(package);
        }


        public void LoadPage(IViewSwitcher viewModel)
        {
            viewModel.SwitchView += OnSwitchView;
            TabPage = viewModel;
        }

        private void OnSwitchView(object sender, ViewEventArgs e)
        {
            OnViewSwitched(e.ViewToLoad, e.ViewType, e.ViewPlace, e.ViewNameToClose);
        }
    }
}
