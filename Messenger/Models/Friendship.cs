namespace Messenger
{
    public class Friendship : BaseModel
    {
        public string Friend1Email { get; set; }
        public User Friend1 { get; set; }
        public string Friend2Email { get; set; }
        public User Friend2 { get; set; }
    }
}
