using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace Messenger
{
    public class Server : BaseModel, IFileHolder, INotifyDataErrorInfo
    {
        public int Id_Server { get; set; }

        private string _name;
        public string Name
        {
            get => _name;

            set
            {
                _name = value;
                ClearError(nameof(Name));
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        private byte[] _file;
        public byte[] File
        {
            get => _file;
            set
            {
                _file = value;
                OnPropertyChanged(nameof(File));
                OnPropertyChanged(nameof(BitmapFile));
            }
        }
        public string InviteCode { get; set; }

        [JsonIgnore]
        public BitmapImage BitmapFile
        {
            get => FileHelper.GetBitmapFromByteArray(File);
        }
        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Role> Roles { get; set; }
        public ObservableCollection<Chat> Chats { get; set; }

        public readonly Dictionary<string, List<string>> _propertyErrors = new();
        public bool HasErrors => _propertyErrors.Any();

        public Server()
        {

        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName) => _propertyErrors.GetValueOrDefault(propertyName, null);

        public void AddError(string propertyName, string errorMessage)
        {
            if (!_propertyErrors.ContainsKey(propertyName))
                _propertyErrors.Add(propertyName, new List<string>());

            _propertyErrors[propertyName].Add(errorMessage);
            OnErrorsChanged(propertyName);
        }

        public void ClearError(string propertyName)
        {
            if (_propertyErrors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }

        public void OnErrorsChanged(string propertyName) =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}
