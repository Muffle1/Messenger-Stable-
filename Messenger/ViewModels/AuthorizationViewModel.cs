using System.Threading.Tasks;
using System.Windows;

namespace Messenger
{
    public class AuthorizationViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand AuthorizationCommand { get; private set; }
        public RelayCommand RegistrationPageCommand { get; private set; }
        #endregion

        public bool RememberUser { get; set; }
        public User UserAuth { get; set; }

        public AuthorizationViewModel()
        {
            UserAuth = new User();
            AuthorizationCommand = new RelayCommand(Auth);
            RegistrationPageCommand = new RelayCommand(OpenRegistrationPage);
        }


        private async void Auth(object obj)
        {
            await Task.Run(() =>
            {
                if (Validation())
                {
                    User user = ServerContext.Authorization(UserAuth);
                    if (user == null)
                        UserAuth.AddError(nameof(UserAuth.Email), "Не правильные email или пароль");
                    else
                    {
                        if (RememberUser)
                            new ConfigurationUserHelper().Write(user);

                        Application.Current.Dispatcher.Invoke(() =>
                            OnViewSwitched(new MainViewModel(user), ViewType.Manager));
                    }
                }
            });
        }

        private void OpenRegistrationPage(object obj) =>
            OnViewSwitched(new RegistrationViewModel(), ViewType.Page);

        /// <summary>
        /// Валидация данных
        /// </summary>
        /// <returns>Возвращение все ли данные прошли валидацию</returns>
        private bool Validation()
        {
            UserAuth.ClearError(nameof(UserAuth.Email));
            if (string.IsNullOrEmpty(UserAuth.Email))
                UserAuth.AddError(nameof(UserAuth.Email), "Не правильный Email");
            if (string.IsNullOrEmpty(UserAuth.Password))
                UserAuth.AddError(nameof(UserAuth.Password), "Не правильный пароль");
            if (UserAuth.HasErrors)
                return false;

            return true;
        }
    }
}
