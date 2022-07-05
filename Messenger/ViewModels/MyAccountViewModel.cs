using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class MyAccountViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand EditProfileCommand { get; private set; }
        public RelayCommand ApplyRequestCommand { get; private set; }
        public RelayCommand CancelRequestCommand { get; private set; }
        #endregion

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

        private string _editBtnText;
        public string EditBtnText
        {
            get => _editBtnText;
            set
            {
                _editBtnText = value;
                OnPropertyChanged(nameof(EditBtnText));
            }
        }
        public ObservableCollection<Request> EnterRequests { get; set; } = new();
        public ObservableCollection<Request> SendRequests { get; set; } = new();

        public MyAccountViewModel(User user)
        {
            CurrentUser = user;
            EditBtnText = "Редактировать";
            EditProfileCommand = new RelayCommand(EditProfile);
            ApplyRequestCommand = new RelayCommand(ApplyRequest);
            CancelRequestCommand = new RelayCommand(CancelRequest);

            Package package = new(Command.GetProfileInfo, CurrentUser.Email);
            ServerContext.SendRequest(package);
        }

        public override void SubscribeEvent()
        {
            ServerContext.EnterRequestCmdReceived += OnEnterRequestCmdReceived;
            ServerContext.RemoveRequestCmdReceived += OnRemoveRequestCmdReceived;
            ServerContext.GetFriendCmdReceived += OnGetFriendCmdReceived;
            ServerContext.GetProfileInfoCmdReceived += OnGetProfileInfoCmdReceived;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.EnterRequestCmdReceived -= OnEnterRequestCmdReceived;
            ServerContext.RemoveRequestCmdReceived -= OnRemoveRequestCmdReceived;
            ServerContext.GetFriendCmdReceived -= OnGetFriendCmdReceived;
            ServerContext.GetProfileInfoCmdReceived -= OnGetProfileInfoCmdReceived;
        }

        private void OnGetProfileInfoCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is List<Request> requests)
            {
                EnterRequests = new ObservableCollection<Request>(requests.Where(r => r.Receiver == CurrentUser.Email));
                SendRequests = new ObservableCollection<Request>(requests.Where(r => r.Sender == CurrentUser.Email));
                OnPropertyChanged(nameof(EnterRequests));
                OnPropertyChanged(nameof(SendRequests));
            }
        }
            
        private void OnGetFriendCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is User friend)
            {
                Request request = EnterRequests.FirstOrDefault(r => r.Sender == friend.Email);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (request != null)
                        EnterRequests.Remove(request);
                    else if ((request = SendRequests.FirstOrDefault(r => r.Receiver == friend.Email)) != null)
                        SendRequests.Remove(request);
                });
            }
        }

        private void OnRemoveRequestCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Request request)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Request requestExist = EnterRequests.FirstOrDefault(r => r.Id_Request == request.Id_Request);
                    if (requestExist != null)
                        EnterRequests.Remove(requestExist);
                    else if ((requestExist = SendRequests.FirstOrDefault(r => r.Id_Request == request.Id_Request)) != null)
                        SendRequests.Remove(requestExist);
                });
            }
        }

        private void OnEnterRequestCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Request request)
                Application.Current.Dispatcher.Invoke(() => EnterRequests.Add(request));
        }

        private void EditProfile(object obj)
        {
            if (EditBtnText == "Готово")
            {
                if (IsUserDataCorrect())
                {
                    Package package = new(Command.ChangeСharacteristics, CurrentUser);
                    ServerContext.SendRequest(package);
                }
                EditBtnText = "Редактировать";
                return;
            }

            EditBtnText = "Готово";
        }

        private void ApplyRequest(object obj)
        {
            Request request = EnterRequests.FirstOrDefault(u => u.Id_Request == (int)obj);
            Package package = new(Command.AddFriend, request);
            ServerContext.SendRequest(package);
        }

        private void CancelRequest(object obj)
        {
            Request request = EnterRequests.FirstOrDefault(r => r.Id_Request == (int)obj);

            if (request is null)
                request = SendRequests.FirstOrDefault(r => r.Id_Request == (int)obj);

            Package package = new(Command.RemoveRequest, request);
            ServerContext.SendRequest(package);
        }

        /// <summary>
        /// Валидация
        /// </summary>
        /// <returns>Возвращает true еслли данные прошли валидацию, иначе возващает false</returns>
        private bool IsUserDataCorrect()
        {
            if ((string.IsNullOrWhiteSpace(CurrentUser.Name)) || (!CurrentUser.Name.All(ch => char.IsLetter(ch))))
                CurrentUser.AddError(nameof(CurrentUser.Name), "Не верное имя!");

            if ((string.IsNullOrWhiteSpace(CurrentUser.SurName)) || (!CurrentUser.SurName.All(ch => char.IsLetter(ch))))
                CurrentUser.AddError(nameof(CurrentUser.SurName), "Не верная фамилия!");

            if ((string.IsNullOrWhiteSpace(CurrentUser.PhoneNumber)) || (CurrentUser.PhoneNumber.Contains('_')))
                CurrentUser.AddError(nameof(CurrentUser.PhoneNumber), "Не верный номер телефона");

            if (CurrentUser.HasErrors)
                return false;

            return true;
        }
    }
}
