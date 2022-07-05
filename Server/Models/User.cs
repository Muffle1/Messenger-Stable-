using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    public class User
    {
        [Key]
        public string Email { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string PhoneNumber { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime Birthday { get; set; }
        [ForeignKey("Settings_Id")]
        public Settings Settings { get; set; }

        [NotMapped]
        public List<User> Friends { get; set; }
        public List<Request> EnterRequests { get; set; }
        public List<Request> SendRequests { get; set; }
        public List<Server> Servers { get; set; }
        public List<Chat> Chats { get; set; }
        public List<UserChat> UserChats { get; set; }
        public List<Role> Roles { get; set; }
    }
}
