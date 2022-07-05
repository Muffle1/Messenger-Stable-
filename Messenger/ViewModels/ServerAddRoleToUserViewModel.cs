using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class ServerAddRoleToUserViewModel : BaseViewModel
    {
        #region Команды
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand AddUserToRoleCommand { get; private set; }
        #endregion

        private readonly List<User> _allUsersServerWithoutRole;

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get => _users;

            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }

        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;

            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(_selectedRole));
            }
        }

        private string _searchString;
        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                if (!string.IsNullOrEmpty(_searchString))
                    Users = new ObservableCollection<User>(_allUsersServerWithoutRole.Where(x => x.Email.ToLower().Contains(_searchString.ToLower())));
                else Users = new ObservableCollection<User>(_allUsersServerWithoutRole);
            }
        }

        private readonly Server _server;

        public ServerAddRoleToUserViewModel(Server server, Role role)
        {
            SelectedRole = role.Clone();
            _server = server;
            _allUsersServerWithoutRole = _server.Users.Where(u => !u.Roles.Any(r => r.Id_Role == SelectedRole.Id_Role)).ToList();
            Users = new ObservableCollection<User>(_allUsersServerWithoutRole);
            CloseCommand = new RelayCommand(Close);
            AddCommand = new RelayCommand(Add);
            AddUserToRoleCommand = new RelayCommand(AddUserToRole);
        }

        private void Close(object obj) =>
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "ServerAddRoleToUserViewModel");

        private void Add(object obj)
        {
            Package package = new(Command.AddUserToRole, SelectedRole);
            ServerContext.SendRequest(package);
            OnViewSwitched(null, ViewType.Popup, ViewPlace.Default, "ServerAddRoleToUserViewModel");
        }

        private void AddUserToRole(object obj)
        {
            if (!SelectedRole.Users.Any(u => u.Email == (string)obj))
                SelectedRole.Users.Add(new User() { Email = (string)obj });
            else
                SelectedRole.Users.Remove(SelectedRole.Users.FirstOrDefault(u => u.Email == (string)obj));
        }

        public override void SubscribeEvent()
        {
            ServerContext.AddUserToRoleCmdReceived += ClientModel_AddUserToRoleCmdReceived;
            ServerContext.RemoveUserFromRoleCmdReceived += ClientModel_RemoveUserFromRoleCmdReceived;
            ServerContext.JoinServerCmdReceived += OnJoinServerCmdReceived;
        }

        private void OnJoinServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is ServerObject serverObject) && (SelectedRole.Server_Id == serverObject.IdServer) && (SelectedRole.Name == "Everyone"))
                Application.Current.Dispatcher.Invoke(() => Users.Add(serverObject.Parameter as User));
        }

        private void ClientModel_RemoveUserFromRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is ServerObject serverObject && (serverObject.Parameter as Tuple<string, int>).Item2 == SelectedRole.Id_Role)
            {
                User user = _server.Users.FirstOrDefault(u => u.Email == (serverObject.Parameter as Tuple<string, int>).Item1);
                _allUsersServerWithoutRole.Add(user);
                Application.Current.Dispatcher.Invoke(() => Users = new ObservableCollection<User>(_allUsersServerWithoutRole));
            }
        }


        private void ClientModel_AddUserToRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Role role && role.Id_Role == SelectedRole.Id_Role)
            {
                foreach (var email in role.Users.Select(u => u.Email))
                {
                    User user = _allUsersServerWithoutRole.FirstOrDefault(u => u.Email == email);
                    _allUsersServerWithoutRole.Remove(user);
                    Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
                }
            }
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.AddUserToRoleCmdReceived -= ClientModel_AddUserToRoleCmdReceived;
            ServerContext.RemoveUserFromRoleCmdReceived -= ClientModel_RemoveUserFromRoleCmdReceived;
            ServerContext.JoinServerCmdReceived -= OnJoinServerCmdReceived;
        }
    }
}
