namespace Messenger
{
    public class MainWindowViewModel : BaseViewModel
    {
        private IViewSwitcher _currentPageManager;
        public IViewSwitcher CurrentPageManager
        {
            get => _currentPageManager;
            set
            {
                if (_currentPageManager != null)
                    (_currentPageManager as IEventSubscriber).UnsubscribeFromEvent();

                _currentPageManager = value;
                (_currentPageManager as IEventSubscriber).SubscribeEvent();

                OnPropertyChanged(nameof(CurrentPageManager));
            }
        }

        public MainWindowViewModel()
        {
            User user = new ConfigurationUserHelper().Read();
            if (!string.IsNullOrEmpty(user.Email))
            {
                user = ServerContext.Authorization(user);
                if (user != null)
                    LoadManager(new MainViewModel(user));
                else LoadManager(new AuthenticationViewModel());
            }
            else
                LoadManager(new AuthenticationViewModel());
        }
        /// <summary>
        /// Загрузка менеджера
        /// </summary>
        /// <param name="viewModel">ViewModel</param>
        public void LoadManager(IViewSwitcher viewModel)
        {
            if (CurrentPageManager is not null)
                CurrentPageManager.SwitchView -= OnSwitchPage;

            viewModel.SwitchView += OnSwitchPage;
            CurrentPageManager = viewModel;
        }
        /// <summary>
        /// Обработка события переключения страниц
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Аргумент</param>
        private void OnSwitchPage(object sender, ViewEventArgs e)
        {
            LoadManager(e.ViewToLoad);
        }
    }
}
