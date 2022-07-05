using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Messenger
{
    class FriendsViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand PopupCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }
        #endregion

        public ObservableCollection<User> Friends { get; set; } = new();

        public User _currentUser;
        public ItemsControl ItemsSourceFriends { get; set; } = new();

        private string _searchEmail;
        public string SearchEmail
        {
            get => _searchEmail;
            set
            {
                _searchEmail = value;
                if (!string.IsNullOrEmpty(_searchEmail))
                    ItemsSourceFriends.ItemsSource = new ObservableCollection<User>(Friends.Where(x => x.Email.ToLower().Contains(_searchEmail)));
                else
                    ItemsSourceFriends.ItemsSource = Friends;
                CountFriends = (ItemsSourceFriends.ItemsSource as IEnumerable<User>).Count();
                OnPropertyChanged(nameof(ItemsSourceFriends.ItemsSource));
                OnPropertyChanged(nameof(SearchEmail));
            }
        }

        private int _countFriends;
        public int CountFriends
        {
            get => _countFriends;
            set
            {
                _countFriends = value;
                OnPropertyChanged(nameof(CountFriends));
            }
        }

        public FriendsViewModel(User user)
        {
            ItemsSourceFriends.ItemsSource = new ObservableCollection<User>();
            _currentUser = user;

            PopupCommand = new RelayCommand(OpenFriendProfilePopup);
            ClearCommand = new RelayCommand(Clear);

            Package package = new(Command.GetFriends, _currentUser.Email);
            ServerContext.SendRequest(package);
        }

        public override void SubscribeEvent()
        {
            ServerContext.GetFriendsCmdReceived += OnGetFriendsCmdReceived;
            ServerContext.RemoveFriendCmdReceived += OnRemoveFriendCmdReceived;
            ServerContext.GetFriendCmdReceived += OnGetFriendCmdReceived;
        }
        public override void UnsubscribeFromEvent()
        {
            ServerContext.GetFriendsCmdReceived -= OnGetFriendsCmdReceived;
            ServerContext.RemoveFriendCmdReceived -= OnRemoveFriendCmdReceived;
            ServerContext.GetFriendCmdReceived -= OnGetFriendCmdReceived;
        }

        private void OnGetFriendCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is User friend)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Friends.Add(friend);
                    CountFriends = (ItemsSourceFriends.ItemsSource as IEnumerable<User>).Count();
                });
            }
        }

        private void OnRemoveFriendCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is string friendEmail)
            {
                User friend = Friends.FirstOrDefault(u => u.Email == friendEmail);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (friend != null)
                    {
                        Friends.Remove(friend);
                        friend = (ItemsSourceFriends.ItemsSource as ObservableCollection<User>).FirstOrDefault(u => u.Email == friendEmail);
                        (ItemsSourceFriends.ItemsSource as ObservableCollection<User>).Remove(friend);
                        CountFriends = (ItemsSourceFriends.ItemsSource as IEnumerable<User>).Count();
                    }
                });
            }
        }

        private void OnGetFriendsCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is List<User> friends)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Friends = new ObservableCollection<User>(friends);
                    ItemsSourceFriends.ItemsSource = Friends;
                    CountFriends = friends.Count;
                    OnPropertyChanged(nameof(ItemsSourceFriends.ItemsSource));
                });
            }
        }

        private void OpenFriendProfilePopup(object obj) =>
            OnViewSwitched(new AccountViewModel(Friends.FirstOrDefault(x => x.Email == (string)obj)), ViewType.Popup);

        private void Clear(object obj) =>
            SearchEmail = "";
    }
}
