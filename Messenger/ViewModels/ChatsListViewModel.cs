using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Messenger
{
    public class ChatsListViewModel : BaseViewModel
    {
        public RelayCommand PopupCommand { get; private set; }
        public RelayCommand FriendsCommand { get; private set; }
        public RelayCommand AddFriendCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }

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

        public ObservableCollection<Chat> Chats { get; set; }

        private Chat _selectedChat;
        public Chat SelectedChat
        {
            get => _selectedChat;
            set
            {
                _selectedChat = value;
                OnPropertyChanged(nameof(SelectedChat));
            }
        }

        private object _selectedSearch;
        public object SelectedSearch
        {
            get => _selectedSearch;
            set
            {
                _selectedSearch = value;

                if (SelectedSearch is Chat chat)
                {
                    if ((SelectedChat == null) || (SelectedChat.Id_Chat != chat.Id_Chat))
                    {
                        OnViewSwitched(new ChatViewModel(CurrentUser, chat), ViewType.Page, ViewPlace.Right);
                        SelectedChat = chat;
                    }
                }
                else
                    SelectedChat = null;

                OnPropertyChanged(nameof(SelectedSearch));
            }
        }

        private string _searchString;
        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                if (!string.IsNullOrEmpty(SearchString))
                {
                    Package package = new(Command.GetSearchedUsers, SearchString);
                    ServerContext.SendRequest(package);
                }
                else
                    ItemControl.ItemsSource = Chats;

                OnPropertyChanged(nameof(ItemControl));
                OnPropertyChanged(nameof(SearchString));
            }
        }

        public ItemsControl ItemControl { get; set; } = new ItemsControl();

        public ChatsListViewModel(User user)
        {
            CurrentUser = user;
            ItemControl.ItemsSource = Chats;

            Package package = new(Command.GetChats, CurrentUser.Email);
            ServerContext.SendRequest(package);

            PopupCommand = new RelayCommand(Popup);
            FriendsCommand = new RelayCommand(Friends);
            AddFriendCommand = new RelayCommand(AddFriend);
            ClearCommand = new RelayCommand(Clear);
            SettingsCommand = new RelayCommand(Settings);
        }

        public override void SubscribeEvent()
        {
            ServerContext.GetChatsCmdReceived += OnGetChatsCmdReceived;
            ServerContext.GetSearchedUsersCmdReceived += OnGetSearchedUsersCmdReceived;
            ServerContext.GetChatCmdReceived += OnGetChatCmdReceived;
            ServerContext.RemoveChatCmdReceived += OnRemoveChatCmdReceived;
            AccountViewModel.OpenChatClick += OnOpenChatClick;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.GetChatsCmdReceived -= OnGetChatsCmdReceived;
            ServerContext.GetSearchedUsersCmdReceived -= OnGetSearchedUsersCmdReceived;
            ServerContext.GetChatCmdReceived -= OnGetChatCmdReceived;
            ServerContext.RemoveChatCmdReceived -= OnRemoveChatCmdReceived;
            AccountViewModel.OpenChatClick -= OnOpenChatClick;
        }

        private void OnGetChatsCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is List<Chat> chats)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Chats = new ObservableCollection<Chat>(chats);
                    ItemControl.ItemsSource = Chats;
                });
            }
        }

        private void OnOpenChatClick(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is User user)
            {
                ItemControl.ItemsSource = Chats;
                SelectedSearch = Chats.FirstOrDefault(c => (!c.UserChats.Any(x => x.IsAdmin))
                                                           && (c.Users.Any(u => u.Email == user.Email)));

                OnViewSwitched(new ChatViewModel(CurrentUser, SelectedChat), ViewType.Page, ViewPlace.Right);
            }
        }

        private void OnGetSearchedUsersCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is User[] users)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    ItemControl.ItemsSource = new ObservableCollection<User>(users));
            }
        }

        private void OnGetChatCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Chat chat)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (chat != null)
                        Chats.Add(chat);
                });

                if (chat.UserChats.Any(uc => (uc.Email == CurrentUser.Email) && (uc.IsAdmin)))
                    OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "ChatCreateViewModel");
            }
        }

        private void OnRemoveChatCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Chat chat && chat.Server_Id is null)
            {
                if (SelectedChat?.Id_Chat == chat.Id_Chat)
                {
                    OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(ChatViewModel));
                    if ((SelectedChat.UserChats.Any(uc => (uc.Email == CurrentUser.Email) && (uc.IsAdmin))))
                        OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(ChatManagementViewModel));
                }
                Application.Current.Dispatcher.Invoke(() =>
                    Chats.Remove(Chats.FirstOrDefault(c => c.Id_Chat == chat.Id_Chat)));
            }
        }

        private void Popup(object obj) =>
            OnViewSwitched(new ChatCreateViewModel(CurrentUser), ViewType.Popup);

        private void Friends(object obj) =>
            OnViewSwitched(new FriendsViewModel(CurrentUser), ViewType.Page, ViewPlace.Right);

        private void AddFriend(object obj)
        {
            if (SelectedSearch is User user)
            {
                Package package = new(Command.SendFriendRequest, new Request(CurrentUser, user));
                ServerContext.SendRequest(package);
            }
        }

        private void Clear(object obj) =>
            SearchString = "";

        private void Settings(object obj) =>
            OnViewSwitched(new SettingsViewModel(CurrentUser), ViewType.Popup);
    }
}
