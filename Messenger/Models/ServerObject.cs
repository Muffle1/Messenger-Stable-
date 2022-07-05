namespace Messenger
{
    public class ServerObject
    {
        public int IdServer { get; set; }
        public object Parameter { get; set; }

        public ServerObject(int idServer, object parameter)
        {
            IdServer = idServer;
            Parameter = parameter;
        }
    }
}
