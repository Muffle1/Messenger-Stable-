using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    public class FrameInfo
    {
        public string Email { get; set; }
        public byte[] Frame { get; set; }

        public FrameInfo(string email)
        {
            Email = email;
        }
    }
}
