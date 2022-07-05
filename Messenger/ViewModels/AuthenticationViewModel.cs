namespace Messenger
{
    //Переназвать viewmodel мб было бы не плохо
    public class AuthenticationViewModel : BaseViewModel
    {
        private IViewSwitcher _currentPage;
        public IViewSwitcher CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public AuthenticationViewModel()
        {
            LoadPage(new AuthorizationViewModel());
        }

        /// <summary>
        /// Загрузка страницы
        /// </summary>
        /// <param name="viewModel">Viewmodel</param>
        public void LoadPage(IViewSwitcher viewModel)
        {
            viewModel.SwitchView += OnSwitchView;
            CurrentPage = viewModel;
        }

        /// <summary>
        /// Переключение страницы
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">Аргумент события</param>
        private void OnSwitchView(object sender, ViewEventArgs e)
        {
            if (e.ViewType == ViewType.Page)
                LoadPage(e.ViewToLoad);
            else if (e.ViewType == ViewType.Manager)
                OnViewSwitched(e.ViewToLoad, ViewType.Manager);
        }
    }
}
