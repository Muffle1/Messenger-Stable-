namespace Messenger
{
    public class SuccessfulRegistrationPageViewModel : BaseViewModel
    {
        public RelayCommand BackPageCommand { get; private set; }

        public SuccessfulRegistrationPageViewModel() =>
            BackPageCommand = new RelayCommand(OpenAuthorizationPage);

        private void OpenAuthorizationPage(object obj) =>
            OnViewSwitched(new AuthorizationViewModel(), ViewType.Page);
    }
}
