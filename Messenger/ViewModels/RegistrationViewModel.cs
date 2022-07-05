using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Messenger
{
    class RegistrationViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand BackCommand { get; private set; }
        public RelayCommand RegistrationPageCommand { get; private set; }
        #endregion

        public User UserReg { get; set; }

        public RegistrationViewModel()
        {
            BackCommand = new RelayCommand(OpenAuthorizationPage);
            RegistrationPageCommand = new RelayCommand(Registrate);

            Days = new int[31];
            Years = new int[121];
            SetValuesInDaysOrYears(Days, 31);
            SetValuesInDaysOrYears(Years, DateTime.Now.Year);
            UserReg = new User();
        }
        /// <summary>
        /// Установка значений в года и в дни
        /// </summary>
        /// <param name="array">Массив цифр</param>
        /// <param name="startvalue">Стартовое значение</param>
        private static void SetValuesInDaysOrYears(int[] array, int startvalue)
        {
            array[0] = startvalue;
            for (int i = 1; i < array.Length; i++)
            {
                array[i] = array[i - 1] - 1;
            }
        }

        private int[] _days;
        public int[] Days
        {
            get => _days;

            set
            {
                _days = value;
                OnPropertyChanged(nameof(Days));
            }
        }

        private int[] _years;
        public int[] Years
        {
            get => _years;

            set
            {
                _years = value;
                OnPropertyChanged(nameof(Years));
            }
        }

        private void OpenAuthorizationPage(object obj) =>
            OnViewSwitched(new AuthorizationViewModel(), ViewType.Page);

        private async void Registrate(object obj)
        {
            await Task.Run(() =>
            {
                if (IsUserDataCorrect(obj))
                {
                    if (!ServerContext.Registration(UserReg))
                    {
                        UserReg.AddError(nameof(UserReg.Email), "Пользователь с таким Email уже существует!");
                        return;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                        OnViewSwitched(new SuccessfulRegistrationPageViewModel(), ViewType.Page));
                }
            });

        }
        /// <summary>
        /// Валидация
        /// </summary>
        /// <param name="o">Длина пароля</param>
        /// <returns>Возвращает true, если данные прошли валидацию, иначе возвращает false</returns>        
        private bool IsUserDataCorrect(object o)
        {
            if ((string.IsNullOrWhiteSpace(UserReg.Name)) || (!UserReg.Name.All(ch => char.IsLetter(ch))))
                UserReg.AddError(nameof(UserReg.Name), "Не верное имя!");

            if ((string.IsNullOrWhiteSpace(UserReg.SurName)) || (!UserReg.SurName.All(ch => char.IsLetter(ch))))
                UserReg.AddError(nameof(UserReg.SurName), "Не верная фамилия!");

            if ((string.IsNullOrWhiteSpace(UserReg.Password)) || ((int)o < 8))
                UserReg.AddError(nameof(UserReg.Password), "Пароль должен быть больше 8 символов!");

            if ((string.IsNullOrWhiteSpace(UserReg.PhoneNumber)) || (UserReg.PhoneNumber.Contains('_')))
                UserReg.AddError(nameof(UserReg.PhoneNumber), "Не верный номер телефона");

            if ((string.IsNullOrWhiteSpace(UserReg.Login)) || (UserReg.Login.Length < 4))
                UserReg.AddError(nameof(UserReg.Login), "Логин должен быть больше 4 символов");

            if (UserReg.Birthday.Year < 1900)
                UserReg.AddError(nameof(UserReg.Birthday), "Неверная дата рождения!");

            if ((string.IsNullOrWhiteSpace(UserReg.Email)) || (!UserReg.Email.Contains('@') || !UserReg.Email.Contains('.') || UserReg.Email.Length < 5))
                UserReg.AddError(nameof(UserReg.Email), "Не правильный Email");

            if (UserReg.HasErrors)
                return false;

            return true;
        }
    }
}
