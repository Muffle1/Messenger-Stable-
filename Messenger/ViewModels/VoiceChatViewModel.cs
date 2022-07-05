using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Messenger
{
    public class VoiceChatViewModel : BaseViewModel
    {
        private const int timerTime = 2000;

        #region Команды
        public RelayCommand ChangeMicStateCommand { get; set; }
        public RelayCommand ChangeCamStateCommand { get; set; }
        public RelayCommand SelectVideoCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }
        #endregion

        public ObservableCollection<VoiceChatMember> UsersInVoiceChat { get; set; } = new();

        private ImageSource _frame;
        public ImageSource Frame
        {
            get => _frame;

            set
            {
                _frame = value;
                OnPropertyChanged(nameof(Frame));
            }
        }

        private string _emailSelectedVideo;

        private readonly User _currentUser;

        private readonly string _answererEmail;
        private readonly Chat _voiceChat;
        private readonly VoiceHelper _voiceHelper;
        private readonly VideoHelper _videoHelper;
        private bool _microOff = false;
        private bool _camOff = true;
        //private Timer _timer;
        //private Task task;

        public VoiceChatViewModel(Chat chat, User currentUser, IEnumerable<IPUser> ipUsers)
        {
            ChangeMicStateCommand = new RelayCommand(ChangeMicState);
            ChangeCamStateCommand = new RelayCommand(ChangeCamState);
            SelectVideoCommand = new RelayCommand(SelectVideo);
            DisconnectCommand = new RelayCommand(Disconnect);

            AddUsers(ipUsers);

            _currentUser = currentUser;
            _emailSelectedVideo = currentUser.Email;
            _voiceChat = chat;
            _voiceHelper = new VoiceHelper(currentUser.Email, UsersInVoiceChat);
            _voiceHelper.StartVoiceRecord();

            _videoHelper = new VideoHelper(currentUser.Email, UsersInVoiceChat);
            _videoHelper.NewFrameRecieved += OnNewFrameRecieved;

            if (chat.IsVoiceChat == false)
                _answererEmail = chat.Users[0]?.Email;
            else
                SetTimer();
        }

        private void OnNewFrameRecieved(string email, ImageSource frame)
        {
            if ((email == _currentUser.Email) && (_camOff) && (frame is not null))
                _camOff = false;

            if (email == _emailSelectedVideo)
            {
                Application.Current.Dispatcher.Invoke(() => Frame = frame);
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                VoiceChatMember user = UsersInVoiceChat.FirstOrDefault(u => u.IPUser.Email == email);
                if (user is not null)
                    user.Frame = frame;
            });
        }

        private void SetTimer()
        {
            /*task = */
            Task.Delay(TimeSpan.FromMilliseconds(timerTime)).ContinueWith((task) => CheckUserRequest());
            //TimerCallback tm = new TimerCallback(CheckUserRequest);
            //_timer = new Timer(tm, null, 0, timerTime);

        }

        private void CheckUserRequest()
        {
            Package package = new Package(Command.CheckUsersInVoiceChat, new GroupCall(_voiceChat, new IPUser(IPHelper.GetIP(), _currentUser.Email)));
            ServerContext.SendRequest(package);
            //_timer.Dispose();
        }

        private void AddUsers(IEnumerable<IPUser> ipUsers)
        {
            foreach (var user in ipUsers)
            {
                if (!UsersInVoiceChat.Any(u => u.IPUser.Email == user.Email))
                    UsersInVoiceChat.Add(new VoiceChatMember(user));
            }
        }

        /// <summary>
        /// Конструктор для подключения к голосовому чату
        /// </summary>
        public VoiceChatViewModel()
        {

        }

        public override void SubscribeEvent()
        {
            ServerContext.CallCancelCmdReceived += OnCallCancelCmdReceived;
            ServerContext.GetIPCmdReceived += OnGetIPCmdReceived;
            ServerContext.CheckUsersInVoiceChatCmdReceived += OnCheckUsersInVoiceChatCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived += OnRemoveUserFromServerCmdReceived;
        }

        private void OnRemoveUserFromServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is ServerObject serverObject && _voiceChat.IsVoiceChat && serverObject.IdServer == _voiceChat.Server_Id && (string)serverObject.Parameter == _currentUser.Email)
                Disconnect(null);
        }

        private void OnCheckUsersInVoiceChatCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is GroupCall groupCall && groupCall.Chat.Id_Chat == _voiceChat.Id_Chat)
            {
                if (!UsersInVoiceChat.Any(u => u.IPUser.Email == groupCall.IpUser.Email))
                {
                    AddUser(groupCall.IpUser);
                    Package package = new Package(Command.GetIP, new Call(groupCall.IpUser.Email, new IPUser(IPHelper.GetIP(), _currentUser.Email)));
                    ServerContext.SendRequest(package);
                }
            }
        }

        private void OnCallCancelCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is string email)
            {
                if (email == _answererEmail)
                {
                    Disconnect(null);
                    return;
                }
                Application.Current.Dispatcher.Invoke(() => UsersInVoiceChat.Remove(UsersInVoiceChat.FirstOrDefault(u => u.IPUser.Email == email)));
            }
        }

        private void OnGetIPCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is IPUser ipUser)
                AddUser(ipUser);
        }

        private void AddUser(IPUser ipUser) =>
            Application.Current.Dispatcher.Invoke(() => UsersInVoiceChat.Add(new VoiceChatMember(ipUser)));

        public override void UnsubscribeFromEvent()
        {
            _voiceHelper.Exit();
            _videoHelper.Exit();
            Package package = new(Command.CallCancel, new CancelCall(UsersInVoiceChat.Where(u => u.IPUser.Email != _currentUser.Email).Select(u => u.IPUser), _voiceChat));
            ServerContext.SendRequest(package);

            ServerContext.CallCancelCmdReceived -= OnCallCancelCmdReceived;
            ServerContext.GetIPCmdReceived -= OnGetIPCmdReceived;
            ServerContext.CheckUsersInVoiceChatCmdReceived -= OnCheckUsersInVoiceChatCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived -= OnRemoveUserFromServerCmdReceived;
        }

        private void ChangeMicState(object obj)
        {
            if (_microOff)
                _voiceHelper.StartVoiceRecord();
            else
                _voiceHelper.StopVoiceRecord();

            _microOff = !_microOff;
        }

        private void ChangeCamState(object obj)
        {
            if (_camOff)
            {
                OnViewSwitched(new SelectCameraViewModel(_videoHelper), ViewType.Popup);
                return;
            }

            _videoHelper.StopVideoRecord();
            _camOff = true;
        }

        private void SelectVideo(object obj)
        {
            _emailSelectedVideo = (string)obj;

            Application.Current.Dispatcher.Invoke(() =>
            {
                UsersInVoiceChat.FirstOrDefault(u => u.IPUser.Email == (string)obj).Frame = null;
                Frame = null;
            });
        }

        private void Disconnect(object obj) =>
            OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(VoiceChatViewModel));
    }
}
