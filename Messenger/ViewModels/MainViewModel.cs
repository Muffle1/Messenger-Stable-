using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand ClosePopup { get; private set; }

        private IViewSwitcher _leftPage;
        public IViewSwitcher LeftPage
        {
            get => _leftPage;
            set
            {
                if (_leftPage != null)
                    (_leftPage as IEventSubscriber).UnsubscribeFromEvent();

                _leftPage = value;
                if (_leftPage != null)
                    (_leftPage as IEventSubscriber).SubscribeEvent();

                OnPropertyChanged(nameof(LeftPage));
            }
        }

        private IViewSwitcher _middlePage;
        public IViewSwitcher MiddlePage
        {
            get => _middlePage;
            set
            {
                if (_middlePage != null)
                    (_middlePage as IEventSubscriber).UnsubscribeFromEvent();

                _middlePage = value;

                if (_middlePage != null)
                    (_middlePage as IEventSubscriber).SubscribeEvent();

                OnPropertyChanged(nameof(MiddlePage));
            }
        }

        private IViewSwitcher _rightPage;
        public IViewSwitcher RightPage
        {
            get => _rightPage;
            set
            {
                if (_rightPage != null)
                    (_rightPage as IEventSubscriber).UnsubscribeFromEvent();

                _rightPage = value;

                if (_rightPage != null)
                    (_rightPage as IEventSubscriber).SubscribeEvent();

                OnPropertyChanged(nameof(RightPage));
            }
        }

        private IViewSwitcher _popupWindow;
        public IViewSwitcher PopupWindow
        {
            get => _popupWindow;
            set
            {
                if (_popupWindow != null)
                    (_popupWindow as IEventSubscriber).UnsubscribeFromEvent();

                _popupWindow = value;

                if (_popupWindow != null)
                    (_popupWindow as IEventSubscriber).SubscribeEvent();

                OnPropertyChanged(nameof(PopupWindow));
            }
        }

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

        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

        public MainViewModel(User user)
        {
            CurrentUser = user;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            Task.Run(() => ServerContext.ReadServerStream(_token));

            LoadPage(new LeftViewModel(CurrentUser, _tokenSource), ViewPlace.Left);
            LoadPage(new ChatsListViewModel(CurrentUser), ViewPlace.Middle);
            LoadPage(new FriendsViewModel(CurrentUser), ViewPlace.Right);

            ClosePopup = new RelayCommand(Close);
        }

        public override void SubscribeEvent()
        {
            ServerContext.EnterRequestCmdReceived += OnEnterRequestCmdReceived;
            ServerContext.MessageCmdReceived += OnMessageCmdReceived;
            ServerContext.ErrorCmdReceived += OnErrorCmdReceived;
            ToastNotificationManagerCompat.OnActivated += OnActivated;
            ServerContext.CallCmdReceived += OnCallCmdReceived;
            ServerContext.CallResponseCmdReceived += OnCallResponseCmdReceived;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.EnterRequestCmdReceived -= OnEnterRequestCmdReceived;
            ServerContext.MessageCmdReceived -= OnMessageCmdReceived;
            ServerContext.ErrorCmdReceived -= OnErrorCmdReceived;
            ToastNotificationManagerCompat.OnActivated -= OnActivated;
            ServerContext.CallCmdReceived -= OnCallCmdReceived;
            ServerContext.CallResponseCmdReceived -= OnCallResponseCmdReceived;
        }

        private void OnMessageCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Message message)
            {
                if ((CurrentUser.Settings.Notice) && (message.FromEmail != CurrentUser.Email))
                    NotifyHelper.Notify(message);
            }
        }

        private void OnEnterRequestCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((CurrentUser.Settings.Notice) && (e.Parameter is Request request))
                NotifyHelper.Notify(request);
        }

        private void OnCallResponseCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is string response)
            {
                if (response.Contains('@'))
                {
                    var listUser = new List<IPUser> { new IPUser(IPHelper.GetIP(), CurrentUser.Email) };
                    Chat chat = new Chat() {Id_Chat= -1, IsVoiceChat = false, Users = new() { new User() { Email = response } } }
;                   LoadPage(new VoiceChatViewModel(chat, CurrentUser, listUser), ViewPlace.Right);
                }
                else LoadPopup(new ErrorViewModel(response));
            }
        }

        private void OnCallCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is IPUser ipUser)
            {
                if (!(PopupWindow is CallsViewModel))
                    LoadPopup(new CallsViewModel(ipUser, CurrentUser));
            }
        }

        private void OnErrorCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is string error)
                LoadPopup(new ErrorViewModel(error));
        }

        /// <summary>
        /// Обработка нажатой кнопки в уведомлениях
        /// </summary>
        /// <param name="e">Аргумент</param>
        private void OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            //TODO: проверять, для какого приложения пришло уведомление
            ToastArguments args = ToastArguments.Parse(e.Argument);

            Request request = JsonSerializer.Deserialize<Request>(args.Get("friendEmail"));

            if (request != null)
            {
                Package package;
                if (args.Contains("action", "accept"))
                {
                    package = new Package(Command.AddFriend, request);
                    ServerContext.SendRequest(package);
                }
                else if (args.Contains("action", "decline"))
                {
                    package = new Package(Command.RemoveRequest, request);
                    ServerContext.SendRequest(package);
                }
            }
        }

        /// <summary>
        /// Загрузка страницы
        /// </summary>
        /// <param name="viewModel">ViewModel</param>
        public void LoadPage(IViewSwitcher viewModel, ViewPlace viewPlace)
        {
            viewModel.SwitchView += OnSwitchView;
            switch (viewPlace)
            {
                case ViewPlace.Left:
                    {
                        LeftPage = viewModel;
                        break;
                    }
                case ViewPlace.Middle:
                    {
                        MiddlePage = viewModel;
                        break;
                    }
                case ViewPlace.Right:
                    {
                        RightPage = viewModel;
                        break;
                    }
            }
        }

        /// <summary>
        /// Загрузка страницы
        /// </summary>
        /// <param name="viewModel">ViewModel</param>
        public void UnloadPage(ViewPlace viewPlace, string viewNameToClose)
        {
            switch (viewPlace)
            {
                case ViewPlace.Left:
                    {
                        if (LeftPage?.GetType().Name == viewNameToClose)
                        {
                            if (LeftPage is not null)
                                LeftPage.SwitchView -= OnSwitchView;
                            LeftPage = null;
                        }
                        break;
                    }
                case ViewPlace.Middle:
                    {
                        if (MiddlePage?.GetType().Name == viewNameToClose)
                        {
                            if (MiddlePage is not null)
                                MiddlePage.SwitchView -= OnSwitchView;
                            MiddlePage = null;
                        }
                        break;
                    }
                case ViewPlace.Right:
                    {
                        if (RightPage?.GetType().Name == viewNameToClose)
                        {
                            if (RightPage is not null)
                                RightPage.SwitchView -= OnSwitchView;
                            RightPage = null;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Загрузка всплывающего окна
        /// </summary>
        /// <param name="viewModel">ViewModel</param>
        public void LoadPopup(IViewSwitcher viewModel)
        {
            viewModel.SwitchView += OnSwitchView;
            PopupWindow = viewModel;
        }

        /// <summary>
        /// Загрузка всплывающего окна
        /// </summary>
        /// <param name="viewModel">ViewModel</param>
        public void UnloadPopup(string viewNameToClose)
        {
            if (PopupWindow?.GetType().Name == viewNameToClose)
            {
                PopupWindow.SwitchView -= OnSwitchView;
                PopupWindow = null;
            }
        }

        /// <summary>
        /// Переключатель страницы
        /// </summary>
        /// <param name="sender">Отправитель</param>
        /// <param name="e">аргумент</param>

        private void OnSwitchView(object sender, ViewEventArgs e)
        {
            switch (e.ViewType)
            {
                case ViewType.Popup:
                    {
                        if (e.ViewToLoad is not null)
                        {
                            LoadPopup(e.ViewToLoad);
                            return;
                        }

                        UnloadPopup(e.ViewNameToClose);
                        break;
                    }
                case ViewType.Page:
                    {
                        if (e.ViewToLoad is not null)
                        {
                            LoadPage(e.ViewToLoad, e.ViewPlace);
                            return;
                        }

                        UnloadPage(e.ViewPlace, e.ViewNameToClose);
                        break;
                    }
                case ViewType.Manager:
                    {
                        OnViewSwitched(e.ViewToLoad, ViewType.Manager);
                        break;
                    }
            }
        }

        private void Close(object obj) =>
            PopupWindow = null;
    }
}
