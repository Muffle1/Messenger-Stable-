namespace Messenger
{
    class SettingsViewModel : BaseViewModel
    {
        public RelayCommand SaveChangesCommand { get; private set; }

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

        public SettingsViewModel(User user)
        {
            CurrentUser = user;

            SaveChangesCommand = new RelayCommand(SaveChanges);
        }

        private void SaveChanges(object obj)
        {
            Package package = new(Command.ChangeСharacteristics, CurrentUser);
            ServerContext.SendRequest(package);
        }

    }
}
