using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class ChatManagementViewModel : BaseViewModel
    {
        #region Комманды
        public RelayCommand RemoveUserFromChatCommand { get; private set; }
        public RelayCommand AddUserToChatCommand { get; private set; }
        public RelayCommand DeleteChatCommand { get; private set; }
        #endregion

        private Chat _chat;
        public Chat Chat
        {
            get => _chat;
            set
            {
                _chat = value;
                OnPropertyChanged(nameof(Chat));
            }
        }

        public ObservableCollection<User> UsersOutsideChat { get; set; }

        public User CurrentUser { get; set; }

        public ChatManagementViewModel(Chat chat, User user)
        {
            Chat = chat;
            CurrentUser = user;
            Chat.Users.Remove(Chat.Users.FirstOrDefault(u => u.Email == CurrentUser.Email));

            Package package = new(Command.GetFriends, CurrentUser.Email);
            ServerContext.SendRequest(package);

            RemoveUserFromChatCommand = new RelayCommand(RemoveUserFromChat);
            AddUserToChatCommand = new RelayCommand(AddUserToChat);
            DeleteChatCommand = new RelayCommand(DeleteChat);
        }

        public override void SubscribeEvent()
        {
            ServerContext.AddUserToChatCmdReceived += OnAddUserToChatCmdReceived;
            ServerContext.RemoveUserFromChatCmdReceived += OnRemoveUserFromChatCmdReceived;
            ServerContext.GetFriendsCmdReceived += OnGetFriendsCmdReceived;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.AddUserToChatCmdReceived -= OnAddUserToChatCmdReceived;
            ServerContext.RemoveUserFromChatCmdReceived -= OnRemoveUserFromChatCmdReceived;
            ServerContext.GetFriendsCmdReceived -= OnGetFriendsCmdReceived;
        }

        private void OnGetFriendsCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is List<User> friends)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UsersOutsideChat = new ObservableCollection<User>(friends.Where(f => !Chat.Users.Any(u => u.Email == f.Email)));
                    OnPropertyChanged(nameof(UsersOutsideChat));
                });
            }
        }

        private void OnRemoveUserFromChatCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is string email)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    User user = Chat.Users.SingleOrDefault(u => u.Email == email);
                    Chat.Users.Remove(user);
                    UsersOutsideChat.Add(user);

                });
            }
        }

        private void OnAddUserToChatCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is string email)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    User user = UsersOutsideChat.SingleOrDefault(u => u.Email == email);
                    Chat.Users.Add(user);
                    UsersOutsideChat.Remove(user);
                });
            }
        }

        private void AddUserToChat(object user)
        {
            UserChatDTO userChat = new UserChatDTO((user as User).Email, Chat.Id_Chat);

            Package package = new(Command.AddUserToChat, userChat);
            ServerContext.SendRequest(package);
        }

        private void RemoveUserFromChat(object user)
        {
            UserChatDTO userChat = new UserChatDTO((user as User).Email, Chat.Id_Chat);

            Package package = new(Command.RemoveUserFromChat, userChat);
            ServerContext.SendRequest(package);
        }

        private void DeleteChat(object obj)
        {
            Package package = new(Command.RemoveChat, Chat.Id_Chat);
            ServerContext.SendRequest(package);
        }

    }
}
