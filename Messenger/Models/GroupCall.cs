using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    public class GroupCall
    {
        public Chat Chat { get; set; }
        public IPUser IpUser {get; set; }

        public GroupCall(Chat chat, IPUser ipUser)
        {
            Chat = chat;
            IpUser = ipUser;
        }
        public GroupCall()
        {

        }
    }
}
