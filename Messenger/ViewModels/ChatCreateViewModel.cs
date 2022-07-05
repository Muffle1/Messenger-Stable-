namespace Messenger
{
    public class ChatCreateViewModel : BaseViewModel
    {
        public RelayCommand CreateChatCommand { get; private set; }
        public User CurrentUser { get; set; }

        //Todo:Проверить нужно ли нам обновление чата(где ему присваиваеться новое значение)
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

        public ChatCreateViewModel(User user)
        {
            Chat = new Chat();
            CurrentUser = user;
            CreateChatCommand = new RelayCommand(CreateChat);
        }

        //Todo:Переделать
        private void CreateChat(object obj)
        {
            if (!string.IsNullOrEmpty(Chat.Name))
            {
                Chat.UserChats.Add(new UserChat() { Email = CurrentUser.Email, IsAdmin = true });
                Package package = new(Command.CreateChat, Chat);
                ServerContext.SendRequest(package);
                //Exit();
            }
        }
    }
}
