using System.Collections.ObjectModel;

namespace Messenger
{
    public class Role : BaseModel
    {
        public int Id_Role { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public int Server_Id { get; set; }
        public Server Server { get; set; }

        public ObservableCollection<Chat> Chats { get; set; } = new();
        public ObservableCollection<User> Users { get; set; } = new();

        public Role()
        {

        }

        public Role Clone()
        {
            return new Role()
            {
                Id_Role = Id_Role,
                Name = Name,
                Server_Id = Server_Id,
                Users = new()
            };
        }
    }
}
