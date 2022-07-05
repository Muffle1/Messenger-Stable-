using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Messenger
{
    public class Chat : BaseModel
    {
        public int Id_Chat { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    User user = Users?.FirstOrDefault(u => u.Email != ServerContext.UserEmail);
                    if (user != null)
                        return $"{user.SurName} {user.Name}";
                }

                return _name;
            }

            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public bool IsVoiceChat { get; set; }

        public ObservableCollection<User> Users { get; set; } = new();
        public ObservableCollection<Role> Roles { get; set; }

        public List<UserChat> UserChats { get; set; } = new();

        public int? Server_Id { get; set; }
        public Server Server { get; set; }

        public Chat()
        {

        }
    }
}
