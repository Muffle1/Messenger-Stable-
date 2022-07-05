using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    public class UserChatDTO
    {
        public string Email { get; private set; }
        public int Id_Chat { get; private set; }

        public UserChatDTO(string email, int id_chat)
        {
            Email = email;
            Id_Chat = id_chat;
        }
    }
}
