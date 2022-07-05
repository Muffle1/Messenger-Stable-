namespace Server
{
    public class IPUser
    {
        public string IP { get; set; }
        public string Email { get; set; }

        public IPUser(string ip, string email)
        {
            IP = ip;
            Email = email;
        }
        public IPUser()
        {

        }
    }

}
