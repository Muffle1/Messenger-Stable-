namespace Messenger
{
    public class Request : BaseModel
    {
        public int Id_Request { get; set; }
        public string Sender { get; set; }
        public User UserSender { get; set; }
        public string Receiver { get; set; }
        public User UserReceiver { get; set; }

        public Request(User sender, User receiver)
        {
            UserSender = sender;
            Sender = UserSender.Email;
            UserReceiver = receiver;
            Receiver = UserReceiver.Email;
        }

        public Request()
        {

        }
    }
}
