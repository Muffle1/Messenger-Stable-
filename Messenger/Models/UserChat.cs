namespace Messenger
{
    public class UserChat : BaseModel
    {
        public string Email { get; set; }
        public User User { get; set; }
        public int Chat_Id { get; set; }
        public Chat Chat { get; set; }
        public bool IsAdmin { get; set; }
    }
}
