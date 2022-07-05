namespace Messenger
{
    public class AddChannelViewModel : BaseViewModel
    {
        public RelayCommand CancelAddingCommand { get; private set; }
        public RelayCommand CreateChannelCommand { get; private set; }

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

        public AddChannelViewModel(Chat chat)
        {
            Chat = chat;
            CancelAddingCommand = new RelayCommand(CancelAdding);
            CreateChannelCommand = new RelayCommand(CreateChannel);
        }

        private void CancelAdding(object obj)
        {
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "AddChannelViewModel");
        }

        private void CreateChannel(object obj)
        {
            Package package = new(Command.CreateChannel, Chat);
            ServerContext.SendRequest(package);

            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "AddChannelViewModel");
        }
    }
}
