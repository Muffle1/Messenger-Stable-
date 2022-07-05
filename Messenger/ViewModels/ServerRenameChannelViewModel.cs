namespace Messenger
{
    public class ServerRenameChannelViewModel : BaseViewModel
    {
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand BackCommand { get; private set; }
        public string NameChannel { get; set; }

        private readonly Chat _chat;

        public ServerRenameChannelViewModel(Chat chat)
        {
            _chat = chat;
            SaveCommand = new RelayCommand(SaveChannel);
            BackCommand = new RelayCommand(ClosePopup);
        }

        private void SaveChannel(object obj)
        {
            Package package = new(Command.RenameChannel, new Chat() { Id_Chat = _chat.Id_Chat, Name = NameChannel, Server_Id = _chat.Server_Id });
            ServerContext.SendRequest(package);
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(ServerRenameChannelViewModel));
        }

        private void ClosePopup(object obj) =>
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(ServerRenameChannelViewModel));
    }
}
