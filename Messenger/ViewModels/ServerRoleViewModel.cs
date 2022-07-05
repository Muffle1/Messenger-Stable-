using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class ServerRoleViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand ChangeRoleChatsCommand { get; private set; }
        public RelayCommand AddMemberCommand { get; private set; }
        public RelayCommand AddRoleCommand { get; private set; }
        public RelayCommand SaveChangeCommand { get; private set; }
        public RelayCommand RemoveRoleCommand { get; private set; }
        public RelayCommand RemoveUserFromRoleCommand { get; private set; }
        #endregion

        private Server _server;
        public Server Server
        {
            get => _server;

            set
            {
                _server = value;
                OnPropertyChanged(nameof(Server));
            }
        }

        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;

            set
            {
                if (_selectedRole != value)
                {
                    if (ChatChanged.Count > 0)
                    {
                        Package package = new(Command.ChangeAccessRole, ChatChanged);
                        ServerContext.SendRequest(package);
                        ChatChanged = new();
                    }
                    _selectedRole = value;
                    UsersInRole = new ObservableCollection<User>(Server.Users.Where(u => u.Roles.Any(r => r.Id_Role == SelectedRole?.Id_Role)));
                    OnPropertyChanged(nameof(SelectedRole));
                    OnPropertyChanged(nameof(Server.Chats));
                }
            }
        }

        private readonly User _currentUser;

        private List<Tuple<Chat, string, Role>> ChatChanged = new();

        private ObservableCollection<User> _usersInRole;
        public ObservableCollection<User> UsersInRole
        {
            get => _usersInRole;

            set
            {
                _usersInRole = value;
                OnPropertyChanged(nameof(UsersInRole));
            }
        }

        public ServerRoleViewModel(Server server, User currentUser)
        {
            _currentUser = currentUser;
            RemoveRoleCommand = new RelayCommand(RemoveRole);
            AddMemberCommand = new RelayCommand(AddMember);
            AddRoleCommand = new RelayCommand(AddRole);
            SaveChangeCommand = new RelayCommand(SaveChange);
            ChangeRoleChatsCommand = new RelayCommand(ChangeRoleChats);
            RemoveUserFromRoleCommand = new RelayCommand(RemoveUserFromRole);
            Server = server;
            SelectedRole = Server.Roles[0];
        }

        private void RemoveUserFromRole(object obj)
        {
            if ((string)obj != _currentUser.Email)
            {
                Package package = new(Command.RemoveUserFromRole, new Tuple<string, int>((string)obj, SelectedRole.Id_Role));
                ServerContext.SendRequest(package);
            }
        }

        public override void SubscribeEvent()
        {
            ServerContext.AddUserToRoleCmdReceived += OnAddUserToRoleCmdReceived;
            ServerContext.RemoveUserFromRoleCmdReceived += OnRemoveUserFromRoleCmdReceived;
            ServerContext.RemoveRoleCmdReceived += OnRemoveRoleCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived += OnRemoveUserFromServerCmdReceived;
            ServerContext.JoinServerCmdReceived += OnJoinServerCmdReceived;
            //ServerChannelListViewModel.UpdateChatAccess += OnUpdateChatAccess;
        }

        public override void UnsubscribeFromEvent()
        {
            if (ChatChanged.Count > 0)
            {
                Package package = new(Command.ChangeAccessRole, ChatChanged);
                ServerContext.SendRequest(package);
            }

            ServerContext.AddUserToRoleCmdReceived -= OnAddUserToRoleCmdReceived;
            ServerContext.RemoveUserFromRoleCmdReceived -= OnRemoveUserFromRoleCmdReceived;
            ServerContext.RemoveRoleCmdReceived -= OnRemoveRoleCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived -= OnRemoveUserFromServerCmdReceived;
            ServerContext.JoinServerCmdReceived -= OnJoinServerCmdReceived;
            //ServerChannelListViewModel.UpdateChatAccess -= OnUpdateChatAccess;
        }

        private void OnRemoveRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Role role && role.Server_Id == Server.Id_Server && SelectedRole == null)
                Application.Current.Dispatcher.Invoke(() => SelectedRole = Server.Roles[0]);
        }

        private void OnRemoveUserFromRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is ServerObject serverObject && (serverObject.Parameter as Tuple<string, int>).Item2 == SelectedRole.Id_Role)
                Application.Current.Dispatcher.Invoke(() => UsersInRole.Remove(UsersInRole.FirstOrDefault(u => u.Email == (serverObject.Parameter as Tuple<string, int>).Item1)));
        }

        private void OnAddUserToRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Role role && role.Id_Role == SelectedRole.Id_Role)
            {
                foreach (var email in role.Users.Select(u => u.Email))
                {
                    User user = Server.Users.FirstOrDefault(u => u.Email == email);
                    Application.Current.Dispatcher.Invoke(() => UsersInRole.Add(user));
                }
            }
        }

        private void OnRemoveUserFromServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is ServerObject serverObject) && serverObject.IdServer == Server.Id_Server && (UsersInRole.FirstOrDefault(u => u.Email == (string)serverObject.Parameter) is User user))
                Application.Current.Dispatcher.Invoke(() => UsersInRole.Remove(user));
        }

        private void OnJoinServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is ServerObject serverObject) && serverObject.IdServer == Server.Id_Server && (SelectedRole.Name == "Everyone"))
                Application.Current.Dispatcher.Invoke(() => UsersInRole.Add(serverObject.Parameter as User));
        }

        private void ChangeRoleChats(object obj)
        {
            var tuple = ChatChanged.FirstOrDefault(c => c.Item1.Id_Chat == (obj as Chat).Id_Chat && c.Item3.Id_Role == SelectedRole.Id_Role);
            if (tuple == null)
            {
                Role role = (obj as Chat).Roles.FirstOrDefault(r => r.Id_Role == SelectedRole.Id_Role);
                if (role != null)
                {
                    ChatChanged.Add(new Tuple<Chat, string, Role>((Chat)obj, "remove", SelectedRole));
                    return;
                }
                ChatChanged.Add(new Tuple<Chat, string, Role>((Chat)obj, "add", SelectedRole));
                return;
            }
            ChatChanged.Remove(tuple);
        }

        private void RemoveRole(object obj)
        {
            if (SelectedRole.Name != "Admin" && SelectedRole.Name != "Everyone")
            {
                Package package = new(Command.RemoveRole, SelectedRole.Id_Role);
                ServerContext.SendRequest(package);
            }

        }
        private void SaveChange(object obj)
        {
            //TODO:желательно сделать вывод ошибки что такое имя не допустимо
            if ((string)obj != "Admin" && (string)obj != "Everyone")
            {
                SelectedRole.Name = (string)obj;
                Package package = new(Command.ChangeRole, SelectedRole);
                ServerContext.SendRequest(package);
            }
        }
        private void AddMember(object obj) =>
            OnViewSwitched(new ServerAddRoleToUserViewModel(Server, SelectedRole), ViewType.Popup);

        private void AddRole(object obj)
        {
            Package package = new(Command.AddRole, Server.Id_Server);
            ServerContext.SendRequest(package);
        }

    }
}
