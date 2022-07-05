using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        public int Id_Role { get; set; }
        public string Name { get; set; }

        public int Server_Id { get; set; }
        [ForeignKey("Server_Id")]
        public Server Server { get; set; }
        public List<Chat> Chats { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}
