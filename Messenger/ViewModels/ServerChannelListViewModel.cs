using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Messenger
{
    public class ServerChannelListViewModel : BaseViewModel
    {
        //public static event Action UpdateChatAccess;
        #region Команды
        public RelayCommand AddChannelCommand { get; private set; }
        public RelayCommand RemoveChannelCommand { get; private set; }
        public RelayCommand OpenManagementCommand { get; private set; }
        public RelayCommand ChangeChannelCommand { get; private set; }
        public RelayCommand LeaveCommand { get; private set; }
        #endregion

        public ObservableCollection<Chat> TextChannels { get; set; } = new();
        public ObservableCollection<Chat> VoiceChannels { get; set; } = new();

        private Server _server;
        public Server Server
        {
            get => _server;

            set
            {
                _server = value;

                if (Server.Chats is null)
                    Server.Chats = new();

                Server.Chats.CollectionChanged += OnCollectionChanged;

                OnPropertyChanged(nameof(Server));
            }
        }

        private readonly User _currentUser;

        private Chat _selectedChannel;
        public Chat SelectedChannel
        {
            get => _selectedChannel;

            set
            {
                _selectedChannel = value;
                OnPropertyChanged(nameof(SelectedChannel));

                if (SelectedChannel != null)
                {
                    if (SelectedChannel.IsVoiceChat)
                    {
                        Package package = new Package(Command.ConnectVoiceChat,
                            new GroupCall(SelectedChannel, new IPUser(IPHelper.GetIP(), _currentUser.Email)));
                        ServerContext.SendRequest(package);
                    }
                    else OnViewSwitched(new ChatServerViewModel(_currentUser, SelectedChannel, GetChatUser()), ViewType.Page, ViewPlace.Right);

                    return;
                }

                OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(ChatServerViewModel));
            }
        }

        public ServerChannelListViewModel(int ServerId, User user)
        {
            _currentUser = user;
            AddChannelCommand = new RelayCommand(AddChannel);
            RemoveChannelCommand = new RelayCommand(RemoveChannel);
            OpenManagementCommand = new RelayCommand(OpenManagement);
            ChangeChannelCommand = new RelayCommand(ChangeChannel);
            LeaveCommand = new RelayCommand(Leave);

            Package package = new(Command.GetServer, ServerId);
            ServerContext.SendRequest(package);
            Server = new Server() { Id_Server = ServerId };
        }

        private void ChangeChannel(object obj) =>
            OnViewSwitched(new ServerRenameChannelViewModel(Server.Chats.FirstOrDefault(c => c.Id_Chat == (int)obj)), ViewType.Popup);

        public override void SubscribeEvent()
        {
            ServerContext.GetServerCmdReceived += OnGetServerCmdReceived;
            ServerContext.CreateChannelCmdReceived += OnCreateChannelCmdReceived;
            ServerContext.RenameChannelCmdReceived += OnRenameChannelCmdReceived;
            ServerContext.RemoveChatCmdReceived += OnRemoveChatCmdReceived;
            ServerContext.AddRoleCmdReceived += OnAddRoleCmdReceived;
            ServerContext.ChangeServerCmdReceived += OnChangeServerCmdReceived;
            ServerContext.ChangeRoleCmdReceived += OnChangeRoleCmdReceived;
            ServerContext.RemoveRoleCmdReceived += OnRemoveRoleCmdReceived;
            ServerContext.ChangeAccessRoleCmdReceived += OnChangeAccessRoleCmdReceived;
            ServerContext.AddUserToRoleCmdReceived += OnAddUserToRoleCmdReceived;
            ServerContext.RemoveUserFromRoleCmdReceived += OnRemoveUserFromRoleCmdReceived;
            ServerContext.GetChatsByRoleCmdReceived += OnGetChatsByRoleCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived += OnRemoveUserFromServerCmdReceived;
            ServerContext.JoinServerCmdReceived += OnJoinServerCmdReceived;
            ServerContext.ConnectVoiceChatCmdReceived += OnConnectVoiceChatCmdReceived;
        }

        public override void UnsubscribeFromEvent()
        {
            ServerContext.GetServerCmdReceived -= OnGetServerCmdReceived;
            ServerContext.CreateChannelCmdReceived -= OnCreateChannelCmdReceived;
            ServerContext.RenameChannelCmdReceived -= OnRenameChannelCmdReceived;
            ServerContext.RemoveChatCmdReceived -= OnRemoveChatCmdReceived;
            ServerContext.AddRoleCmdReceived -= OnAddRoleCmdReceived;
            ServerContext.ChangeServerCmdReceived -= OnChangeServerCmdReceived;
            ServerContext.ChangeRoleCmdReceived -= OnChangeRoleCmdReceived;
            ServerContext.RemoveRoleCmdReceived -= OnRemoveRoleCmdReceived;
            ServerContext.ChangeAccessRoleCmdReceived -= OnChangeAccessRoleCmdReceived;
            ServerContext.AddUserToRoleCmdReceived -= OnAddUserToRoleCmdReceived;
            ServerContext.RemoveUserFromRoleCmdReceived -= OnRemoveUserFromRoleCmdReceived;
            ServerContext.GetChatsByRoleCmdReceived -= OnGetChatsByRoleCmdReceived;
            ServerContext.RemoveUserFromServerCmdReceived -= OnRemoveUserFromServerCmdReceived;
            ServerContext.JoinServerCmdReceived -= OnJoinServerCmdReceived;
            ServerContext.ConnectVoiceChatCmdReceived -= OnConnectVoiceChatCmdReceived;
        }

        private void OnConnectVoiceChatCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is GroupCall groupCall && groupCall.Chat.Id_Chat == SelectedChannel.Id_Chat)
            {
                OnViewSwitched(new VoiceChatViewModel(SelectedChannel, _currentUser, new IPUser[1] { new IPUser(IPHelper.GetIP(), _currentUser.Email) }), ViewType.Page, ViewPlace.Right);
            }
        }

        private void OnRenameChannelCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Chat chat && chat.Server_Id == Server.Id_Server)
            {
                Chat sChat = Server.Chats.FirstOrDefault(c => c.Id_Chat == chat.Id_Chat);
                sChat.Name = chat.Name;
                OnPropertyChanged(nameof(sChat));
            }
        }

        private void OnGetChatsByRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            //Сделал модель потому что количесто чатов может быть равно 0
            if (e.Parameter is ServerObject serverObject && serverObject.IdServer == Server.Id_Server)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var chat in serverObject.Parameter as List<Chat>)
                    {
                        if (!Server.Chats.Any(c => c.Id_Chat == chat.Id_Chat))
                            Server.Chats.Add(chat);
                    }
                });
            }
        }

        private void OnRemoveUserFromRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is ServerObject serverObject && serverObject.IdServer == Server.Id_Server)
            {
                User user = Server.Users.FirstOrDefault(u => u.Email == (serverObject.Parameter as Tuple<string, int>).Item1);
                Role role = user.Roles.FirstOrDefault(r => r.Id_Role == (serverObject.Parameter as Tuple<string, int>).Item2);
                user.Roles.Remove(role);

                if (user.Email == _currentUser.Email)
                {
                    if (role.Name == "Admin")
                        OnViewSwitched(null, ViewType.Page, ViewPlace.Right, nameof(ServerManagementViewModel));

                    for (int i = 0; i < Server.Chats.Count; ++i)
                    {
                        if (!Server.Chats[i].Roles.Any(cr => user.Roles.Any(ur => ur.Id_Role == cr.Id_Role)))
                        {
                            Chat chatToRemove = Server.Chats.FirstOrDefault(c => c.Id_Chat == Server.Chats[i].Id_Chat);

                            if (SelectedChannel?.Id_Chat == chatToRemove?.Id_Chat)
                                SelectedChannel = null;

                            Application.Current.Dispatcher.Invoke(() => Server.Chats.Remove(chatToRemove));
                            --i;
                        }
                    }
                }
            }
        }

        private void OnAddUserToRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Role role && role.Server_Id == Server.Id_Server)
            {
                foreach (var email in role.Users.Select(u => u.Email))
                {
                    User user = Server.Users.FirstOrDefault(u => u.Email == email);
                    if (user.Roles.FirstOrDefault(r => r.Id_Role == role.Id_Role) == null)
                        user.Roles.Add(role);
                }
                Package package = new(Command.GetChatsByRole, role);
                ServerContext.SendRequest(package);
            }
        }

        //TODO: добавить обновление интерфейса на странице ServerRolePage
        private void OnChangeAccessRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is List<Tuple<Chat, string, Role>> tuple && tuple[0].Item1.Server_Id == Server.Id_Server)
            {
                User user = Server.Users.FirstOrDefault(u => u.Email == _currentUser.Email);
                if (user.Roles.Any(r => r.Id_Role == tuple[0].Item3.Id_Role))
                {
                    Package package = new(Command.GetChatsByRole, tuple[0].Item3);
                    ServerContext.SendRequest(package);
                }

                foreach (var t in tuple)
                {
                    if (Server.Chats.FirstOrDefault(c => c.Id_Chat == t.Item1.Id_Chat) is Chat chat)
                    {
                        if ((t.Item2 == "add") && (!chat.Roles.Any(r => r.Id_Role == t.Item3.Id_Role)))
                        {
                            chat.Roles.Add(t.Item3);
                            continue;
                        }
                        chat.Roles.Remove(chat.Roles.FirstOrDefault(r => r.Id_Role == t.Item3.Id_Role));

                        if (!user.Roles.Any(ur => chat.Roles.Any(cr => ur.Id_Role == cr.Id_Role)))
                            Application.Current.Dispatcher.Invoke(() => Server.Chats.Remove(chat));
                    }
                }
            }
        }

        private void OnCreateChannelCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is Chat chat) && (Server.Id_Server == chat.Server_Id))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Server.Chats.Add(chat);

                    foreach (var user in Server.Users)
                    {
                        foreach (var role in user.Roles.Where(ur => chat.Roles.Any(cr => cr.Id_Role == ur.Id_Role)))
                        {
                            if (!role.Chats.Any(c => c.Id_Chat == chat.Id_Chat))
                                role.Chats.Add(new Chat() { Id_Chat = chat.Id_Chat });
                        }
                    }
                });
            }
        }

        private void OnGetServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is Server server) && (server.Id_Server == Server.Id_Server))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Server = new()
                    {
                        Id_Server = server.Id_Server,
                        Name = server.Name,
                        File = server.File,
                        FileName = server.FileName,
                        InviteCode = server.InviteCode,
                        Users = server.Users,
                        Roles = server.Roles,
                        Chats = new()
                    };

                    foreach (var chat in server.Chats)
                    {
                        if (!Server.Chats.Any(c => c.Id_Chat == chat.Id_Chat))
                            Server.Chats.Add(chat);
                    }

                    OnPropertyChanged(nameof(TextChannels));
                    OnPropertyChanged(nameof(VoiceChannels));
                    OnPropertyChanged(nameof(Server.Users));
                    SelectedChannel = TextChannels.FirstOrDefault();
                });
            }
        }

        private void OnRemoveChatCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Chat chat && chat.Server_Id == Server.Id_Server)
                Application.Current.Dispatcher.Invoke(() => Server.Chats.Remove(Server.Chats.FirstOrDefault(c => c.Id_Chat == chat.Id_Chat)));
        }

        private void OnAddRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Role role && role.Server_Id == Server.Id_Server)
                Application.Current.Dispatcher.Invoke(() => Server.Roles.Add(role));
        }

        //TODO:сделать обновление интерфейса на странице ServerRolePage
        private void OnRemoveRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Role role && Server.Id_Server == role.Server_Id)
            {
                foreach (var user in Server.Users)
                    user.Roles.Remove(user.Roles.FirstOrDefault(r => r.Id_Role == role.Id_Role));

                foreach (var chat in Server.Chats)
                    chat.Roles.Remove(chat.Roles.FirstOrDefault(r => r.Id_Role == role.Id_Role));

                //TODO: пофиксить удаление роли (удаляются чаты)
                User currentUser = Server.Users.FirstOrDefault(u => u.Email == _currentUser.Email);
                List<Chat> chats = Server.Chats.Where(c => !currentUser.Roles.Any(r => c.Roles.Any(cr => cr.Id_Role == r.Id_Role))).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Server.Roles.Remove(Server.Roles.FirstOrDefault(r => r.Id_Role == role.Id_Role));

                    foreach (Chat chat in chats)
                        Server.Chats.Remove(chat);
                });
            }
        }

        private void OnChangeRoleCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is Role role && Server.Id_Server == role.Server_Id)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Role oldRole = Server.Roles.FirstOrDefault(r => r.Id_Role == role.Id_Role);
                    oldRole.Name = role.Name;
                });
            }
        }

        private void OnChangeServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is Server server) && (server.Id_Server == Server.Id_Server))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Server.Name = server.Name;
                    Server.File = server.File;
                    Server.FileName = server.FileName;
                });
            }
        }

        private void OnRemoveUserFromServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if ((e.Parameter is ServerObject serverObject) && (serverObject.IdServer == Server.Id_Server) && ((string)serverObject.Parameter != _currentUser.Email))
                Application.Current.Dispatcher.Invoke(() => Server.Users.Remove(Server.Users.FirstOrDefault(u => u.Email == (string)serverObject.Parameter)));
        }

        private void OnJoinServerCmdReceived(object sender, ServerCmdEventArgs e)
        {
            if (e.Parameter is ServerObject serverObject && serverObject.IdServer == Server.Id_Server)
                Application.Current.Dispatcher.Invoke(() => Server.Users.Add(serverObject.Parameter as User));
        }

        private void AddChannel(object channelType)
        {
            if (Server.Users.Any(u => (u.Email == _currentUser.Email) && (u.Roles.Any(r => r.Name == "Admin"))))
            {
                Chat chat = new()
                {
                    IsVoiceChat = (string)channelType == "VoiceChat",
                    Server_Id = Server.Id_Server
                };
                OnViewSwitched(new AddChannelViewModel(chat), ViewType.Popup);
                return;
            }

            OnViewSwitched(new ErrorViewModel("Для добавления канала необходимо запросить права администратора"), ViewType.Popup);
        }

        private void RemoveChannel(object channel_id)
        {
            Package package = new(Command.RemoveChat, channel_id);
            ServerContext.SendRequest(package);
        }


        private void OpenManagement(object obj)
        {
            Role role = Server.Roles.FirstOrDefault(r => r.Name == "Admin");
            if (Server.Users.FirstOrDefault(u => u.Email == _currentUser.Email).Roles.Any(r => r.Id_Role == role.Id_Role))
                OnViewSwitched(new ServerManagementViewModel(Server, _currentUser), ViewType.Page, ViewPlace.Right);
        }

        private void Leave(object obj)
        {
            if ((Server.Users.Any(u => (u.Email == _currentUser.Email) && (u.Roles.Any(r => r.Name == "Admin"))))
                && (Server.Users.Where(u => u.Roles.Any(r => r.Name == "Admin")).Count() == 1))
            {
                OnViewSwitched(new ErrorViewModel("Необходимо назначить роль Admin хотя бы одному пользователю"), ViewType.Popup);
                return;
            }

            Role role = Server.Roles.FirstOrDefault(r => r.Name == "Everyone");
            Package package = new(Command.RemoveUserFromRole, new Tuple<string, int>(_currentUser.Email, role.Id_Role));
            ServerContext.SendRequest(package);
        }

        private ObservableCollection<User> GetChatUser()
        {
            ObservableCollection<User> users = new();
            foreach (var user in Server.Users)
            {
                if (user.Roles.Any(ur => SelectedChannel.Roles.Any(cr => ur.Id_Role == cr.Id_Role)))
                    users.Add(user);
            }
            return users;
        }

        private void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (Chat chat in e.NewItems)
                {
                    if ((!VoiceChannels.Any(c => c.Id_Chat == chat.Id_Chat)) && (!TextChannels.Any(c => c.Id_Chat == chat.Id_Chat)))
                    {
                        if (chat.IsVoiceChat)
                        {
                            VoiceChannels.Add(chat);
                            return;
                        }

                        TextChannels.Add(chat);
                    }
                }
            }
            else if (e.OldItems is not null)
            {
                foreach (Chat chat in e.OldItems)
                {
                    if (chat.IsVoiceChat)
                    {
                        VoiceChannels.Remove(chat);
                        return;
                    }

                    TextChannels.Remove(chat);
                }
            }
        }
    }
}
