namespace Messenger
{
    public delegate void ViewHandler(object sender, ViewEventArgs e);
    public interface IViewSwitcher
    {
        public event ViewHandler SwitchView;
    }
}
