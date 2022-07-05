namespace Messenger
{
    public class AccountViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand OpenChatCommand { get; private set; }
        public RelayCommand RemoveFriendCommand { get; private set; }
        #endregion

        public static event ServerCmdEventHandler OpenChatClick;

        public User SelectedUser { get; set; }


        public AccountViewModel(User selectedUser)
        {
            SelectedUser = selectedUser;

            Package package = new(Command.GetUser, selectedUser.Email);
            ServerContext.SendRequest(package);

            RemoveFriendCommand = new RelayCommand(RemoveFriend);
            OpenChatCommand = new RelayCommand(OpenChat);
        }

        public override void SubscribeEvent()
        {
            ServerContext.GetUserCmdReceived += OnGetUserCmdReceived;
            ServerContext.RemoveFriendCmdReceived += OnRemoveFriendCmdReceived;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.GetUserCmdReceived -= OnGetUserCmdReceived;
            ServerContext.RemoveFriendCmdReceived -= OnRemoveFriendCmdReceived;
        }

        private void OnRemoveFriendCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is string friendEmail) && (SelectedUser.Email == friendEmail))
                OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "AccountViewModel");
        }

        private void OnGetUserCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is User selectedUser)
            {
                SelectedUser = selectedUser;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        private void OpenChat(object obj)
        {
            OpenChatClick?.Invoke(this, new ServerCmdEventArgs(SelectedUser));
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "AccountViewModel");
        }

        private void RemoveFriend(object obj)
        {
            Package package = new(Command.RemoveFriend, SelectedUser);
            ServerContext.SendRequest(package);
        }

    }
}
