using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    [Table("UsersInChats")]
    public class UserChat
    {
        public string Email { get; set; }
        public User User { get; set; }
        public int Chat_Id { get; set; }
        public Chat Chat { get; set; }
        public bool IsAdmin { get; set; }
    }
}
