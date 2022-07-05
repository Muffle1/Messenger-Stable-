using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class ChatServerViewModel : BaseViewModel
    {
        private IViewSwitcher _chatPage;
        public IViewSwitcher ChatPage
        {
            get => _chatPage;
            set
            {
                if (_chatPage != null)
                    (_chatPage as IEventSubscriber).UnsubscribeFromEvent();

                _chatPage = value;
                if (_chatPage != null)
                    (_chatPage as IEventSubscriber).SubscribeEvent();

                OnPropertyChanged(nameof(ChatPage));
            }
        }

        public ObservableCollection<User> Users { get; set; }

        private Chat _currentChat;

        public ChatServerViewModel(User user, Chat chat, ObservableCollection<User> users)
        {
            LoadPage(new ChatViewModel(user, chat));
            Users = users;
            _currentChat = chat;
            OnPropertyChanged(nameof(Users));
        }

        public override void SubscribeEvent()
        {
            ServerContext.RemoveUserFromServerCmdReceived += OnRemoveUserFromServerCmdReceived;
            ServerContext.JoinServerCmdReceived += OnJoinServerCmdReceived;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.RemoveUserFromServerCmdReceived -= OnRemoveUserFromServerCmdReceived;
            ServerContext.JoinServerCmdReceived -= OnJoinServerCmdReceived;
        }

        private void OnRemoveUserFromServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is ServerObject serverObject) && (Users.FirstOrDefault(u => u.Email == (string)serverObject.Parameter) is User user))
                Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void OnJoinServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is ServerObject serverObject) && serverObject.IdServer == _currentChat.Server_Id && (_currentChat.Roles.Any(r => r.Name == "Everyone")))
                Application.Current.Dispatcher.Invoke(() => Users.Add(serverObject.Parameter as User));
        }

        public void LoadPage(IViewSwitcher viewModel)
        {
            viewModel.SwitchView += OnSwitchView;
            ChatPage = viewModel;
        }

        private void OnSwitchView(object sender, ViewEventArgs e)
        {
            OnViewSwitched(e.ViewToLoad, e.ViewType, e.ViewPlace, e.ViewNameToClose);
        }
    }
}
