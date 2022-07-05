namespace Messenger
{
    public class ViewEventArgs
    {
        public IViewSwitcher ViewToLoad { get; set; }
        public ViewType ViewType { get; set; }
        public ViewPlace ViewPlace { get; set; }
        public string ViewNameToClose { get; set; }

        public ViewEventArgs(IViewSwitcher viewToLoad, ViewType viewType, ViewPlace viewPlace, string viewNameToClose)
        {
            ViewToLoad = viewToLoad;
            ViewType = viewType;
            ViewPlace = viewPlace;
            ViewNameToClose = viewNameToClose;
        }
    }
}
