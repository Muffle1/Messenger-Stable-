using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class CallsViewModel : BaseViewModel
    {
        public RelayCommand CancelCallsCommand { get; set; }
        public RelayCommand AcceptCallCommand { get; set; }
        public ObservableCollection<IPUser> Calls { get; set; }
        private User CurrentUser { get; set; }
        public CallsViewModel(IPUser ipUser, User currentUser)
        {
            CurrentUser = currentUser;
            CancelCallsCommand = new RelayCommand(CancelCalls);
            AcceptCallCommand = new RelayCommand(AcceptCall);
            Calls = new();
            Calls.Add(ipUser);
        }


        public override void SubscribeEvent()
        {
            ServerContext.CallCmdReceived += OnCallCmdReceived;
            ServerContext.CallCancelCmdReceived += OnCallCancelCmdReceived;
        }


        private void OnCallCancelCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is string email)
            {
                Application.Current.Dispatcher.Invoke(() => Calls.Remove(Calls.FirstOrDefault(c => c.Email == email)));
                if (Calls.Count == 0)
                    OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(CallsViewModel));
            }
        }

        private void OnCallCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is IPUser ipUser)
                Application.Current.Dispatcher.Invoke(() => Calls.Add(ipUser));
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.CallCmdReceived -= OnCallCmdReceived;
            ServerContext.CallCancelCmdReceived -= OnCallCancelCmdReceived;

            CancelCall cancelCall = new CancelCall(Calls, new Chat() { IsVoiceChat = false });
            Package package = new(Command.CallCancel, cancelCall);
            ServerContext.SendRequest(package);
        }

        public void CancelCalls(object obj) =>
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(CallsViewModel));

        public void AcceptCall(object obj)
        {
            IPUser caller = new(IPHelper.GetIP(), CurrentUser.Email);
            Call call = new((string)obj, caller);

            Package package = new(Command.GetIP, call);
            ServerContext.SendRequest(package);

            IPUser user = Calls.FirstOrDefault(c => c.Email == (string)obj);
            Chat chat = new Chat() {Id_Chat=-1, IsVoiceChat = false, Users = new() { new User() { Email = (string)obj } } };
            OnViewSwitched(new VoiceChatViewModel(chat, CurrentUser, new List<IPUser> { caller, user }), ViewType.Page, ViewPlace.Right);

            Calls.Remove(user);
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, nameof(CallsViewModel));
        }
    }
}
