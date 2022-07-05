using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Messenger
{
    public class VoiceChatMember : BaseModel
    {
        public IPUser IPUser { get; set; }

        private ImageSource _frame;
        public ImageSource Frame
        {
            get => _frame;

            set
            {
                _frame = value;
                OnPropertyChanged(nameof(Frame));
            }
        }

        public VoiceChatMember(IPUser ipUser)
        {
            IPUser = ipUser;
        }
    }
}
