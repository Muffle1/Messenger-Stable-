using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    [Table("Chats")]
    public class Chat
    {
        [Key]
        public int Id_Chat { get; set; }
        public string Name { get; set; }
        public bool IsVoiceChat { get; set; }

        public List<User> Users { get; set; } = new();
        public List<Message> Messages { get; set; }
        public List<Role> Roles { get; set; }
        public List<UserChat> UserChats { get; set; } = new();

        public int? Server_Id { get; set; }
        [ForeignKey("Server_Id")]
        public Server Server { get; set; }

        public Chat(string name, bool isVoiceChat = false)
        {
            Name = name;
            IsVoiceChat = isVoiceChat;
        }

        public Chat()
        {

        }
    }
}
