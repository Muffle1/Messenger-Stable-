namespace Messenger
{
    public class ErrorViewModel : BaseViewModel
    {
        public RelayCommand CloseCommand { get; set; }
        public string Error { get; set; }
        public ErrorViewModel(string error)
        {
            CloseCommand = new RelayCommand(Close);
            Error = error;
        }

        private void Close(object obj) =>
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "ErrorViewModel");
    }
}
