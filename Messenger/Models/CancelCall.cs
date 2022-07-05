using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    public class CancelCall
    {
        public IEnumerable<IPUser> Users { get; set; }
        public Chat Chat { get; set; }

        public CancelCall(IEnumerable<IPUser> users, Chat chat)
        {
            Users = users;
            Chat = chat;
        }
    }
}
