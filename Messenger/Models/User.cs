using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Messenger
{
    public class User : BaseModel, INotifyDataErrorInfo
    {
        public readonly Dictionary<string, List<string>> _propertyErrors = new();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private string _login;
        public string Login
        {
            get => _login;
            set => SetValue(ref _login, value, nameof(Login));
        }

        private string _passsword;
        public string Password
        {
            get => _passsword;

            set
            {
                _passsword = value;
                ClearError(nameof(Password));
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetValue(ref _name, value, nameof(Name));
        }

        private string _surName;
        public string SurName
        {
            get => _surName;
            set => SetValue(ref _surName, value, nameof(SurName));
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetValue(ref _phoneNumber, value, nameof(PhoneNumber));
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetValue(ref _email, value, nameof(Email));
        }

        private DateTime _birthday;
        public DateTime Birthday
        {
            get => _birthday;
            set
            {
                ClearError(nameof(Birthday));

                if (value.Hour > 0)
                    ValidationDate(_birthday.ToString("dd") + "." + _birthday.ToString("MM") + "." + value.ToString("yyyy"));
                else if (value.Minute > 0)
                    ValidationDate(_birthday.ToString("dd") + "." + value.ToString("MM") + "." + _birthday.ToString("yyyy"));
                else if (value.Second > 0)
                    ValidationDate(value.ToString("dd") + "." + _birthday.ToString("MM") + "." + _birthday.ToString("yyyy"));
                else
                    _birthday = value;

                OnPropertyChanged(nameof(Birthday));
            }
        }

        public Settings Settings { get; set; } = new();

        public ObservableCollection<Server> Servers { get; set; }
        public ObservableCollection<Chat> Chats { get; set; }
        public ObservableCollection<Role> Roles { get; set; }

        public List<UserChat> ChatUsers { get; set; } = new();

        /// <summary>
        /// Валидация даты
        /// </summary>
        /// <param name="date">Дата</param>
        private void ValidationDate(string date)
        {
            if (DateTime.TryParse(date, out DateTime result) == false)
                AddError(nameof(Birthday), "Неверная дата");
            else
                _birthday = result;
        }

        public User()
        {

        }

        //private string Hash(string input) =>
        //    Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input)));

        /// <summary>
        /// Установка значения в поле
        /// </summary>
        /// <typeparam name="T">Тип поля</typeparam>
        /// <param name="field">Поле</param>
        /// <param name="value">Значение</param>
        /// <param name="propertyName">Имя свойства</param>
        public void SetValue<T>(ref T field, T value, string propertyName)
        {
            field = value;
            ClearError(propertyName);
        }

        #region Для валидации
        public bool HasErrors => _propertyErrors.Any();

        /// <summary>
        /// Получения ошибок
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        public IEnumerable GetErrors(string propertyName) => _propertyErrors.GetValueOrDefault(propertyName, null);

        /// <summary>
        /// Добавление ошибки к свойству
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="errorMessage">Сообщение ошибки</param>
        public void AddError(string propertyName, string errorMessage)
        {
            if (!_propertyErrors.ContainsKey(propertyName))
                _propertyErrors.Add(propertyName, new List<string>());

            _propertyErrors[propertyName].Add(errorMessage);
            OnErrorsChanged(propertyName);
        }

        /// <summary>
        /// Очистка ошибки в свойстве
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        public void ClearError(string propertyName)
        {
            if (_propertyErrors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }

        /// <summary>
        /// Вызов события при изменении ошибки
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        public void OnErrorsChanged(string propertyName) =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        #endregion
    }
}
