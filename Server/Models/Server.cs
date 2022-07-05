using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    public class Server
    {
        [Key]
        public int Id_Server { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        [NotMapped]
        public byte[] File { get; set; }
        public string InviteCode { get; set; }
        public List<User> Users { get; set; } = new();
        public List<Role> Roles { get; set; }
        public List<Chat> Chats { get; set; }
    }
}
