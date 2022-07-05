using System;

namespace Messenger
{
    public class ServerCmdEventArgs : EventArgs
    {
        public object Parameter { get; set; }
        public ServerCmdEventArgs(object parameter)
        {
            Parameter = parameter;
        }
    }
}
