using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;

namespace Server
{
    public class ClientContext
    {
        private readonly string _pathSavedFiles;
        private readonly string _pathSavedServerIcon;
        public string UserEmail { get; set; }
        public NetworkStream Stream { get; set; }
        private readonly TcpClient _client;
        private readonly ServerContext _server;
        private BinaryReader _reader;

        public ClientContext(TcpClient client, ServerContext server)
        {
            _pathSavedFiles = @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Server\Messages";
            _pathSavedServerIcon = @$"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Server\Servers";
            _client = client;
            _server = server;
            _server.AddConnection(this);
        }

        /// <summary>
        /// Выполнения клиента на сервере
        /// </summary>
        public void Process()
        {
            try
            {
                Stream = _client.GetStream();
                _reader = new BinaryReader(Stream);
                Package package= GetRequest<Package>();
                switch (package.Command)
                {
                    case Command.Register:
                        {
                            User user = package.GetValue<User>();
                            Registration(user);
                            break;
                        }
                    case Command.Enter:
                        {
                            User user = package.GetValue<User>();
                            Enter(user);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Close();
            }
        }

        private void Registration(User user)
        {
            Package package;
            if (_server.DataBaseHelper.AddUser(user) is User)
                package = new(Command.Register, "Approved");
            else
                package = new(Command.Register, "Denied");

            _server.SendResponse(package, UserEmail);
        }

        private void Enter(User user)
        {
            user = _server.DataBaseHelper.GetUser(user.Email, user.Password);
            Package package;
            if (user != null)
            {
                UserEmail = user.Email;
                LoadServerIcons(user.Servers);
                package = new(Command.Enter, user);
                _server.SendResponse(package, UserEmail);
                LifeCycleClient();
            }
            else
            {
                package = new(Command.Enter, default(User));
                _server.SendResponse(package, UserEmail);
            }

            Close();
        }

        private void LoadServerIcons(List<Server> servers)
        {
            foreach (var server in servers)
                LoadServerIcon(server);
        }

        private void LoadServerIcon(Server server)
        {
            if (!string.IsNullOrEmpty(server.FileName))
                server.File = FileHelper.LoadFile(_pathSavedServerIcon, $"{server.Id_Server}.{server.FileName}");
        }

        private void LifeCycleClient()
        {
            while (true)
            {
                try
                {
                    if (GetMessage() == false)
                        break;
                }
                catch (Exception ex)
                {
                    //Почему тут покинул чат, мб выводить в консоль ошибку?
                    Console.WriteLine($"{UserEmail}: покинул чат");
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        /// <summary>
        /// Получение сообщений
        /// </summary>
        /// <returns>Успешно ли получено письмо</returns>
        private bool GetMessage()
        {
            try
            {
                Package pckg = GetRequest<Package>(); 
                switch (pckg.Command)
                {
                    case Command.GetSearchedUsers:
                        {
                            string search = pckg.GetValue<string>();
                            if (_server.DataBaseHelper.GetSearchUsers(search, UserEmail) is User[] users)
                            {
                                Package package = new(Command.GetSearchedUsers, users);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.ChangeСharacteristics:
                        {
                            User user = pckg.GetValue<User>();
                            _server.DataBaseHelper.SaveChangeUser(user);
                            return true;
                        }
                    case Command.SendFriendRequest:
                        {
                            Request request = pckg.GetValue<Request>();
                            request = _server.DataBaseHelper.AddRequest(request);
                            if (request != null)
                            {
                                Package package = new(Command.SendFriendRequest, request);
                                _server.SendResponse(package, UserEmail);

                                package = new(Command.Notify, request);
                                _server.SendResponse(package,request.Receiver);
                            }
                            return true;
                        }
                    case Command.AddFriend:
                        {
                            Request request = pckg.GetValue<Request>();
                            Chat chat = _server.DataBaseHelper.CreateFriendship(request);

                            Package package;
                            //Отправка созданного чата  и друга пользователю который принял заявку
                            if (_server.DataBaseHelper.GetUser(request.Sender) is User sender)
                            {
                                package = new(Command.GetFriend, sender);
                                _server.SendResponse(package, request.Receiver);

                                package = new(Command.GetChat, chat);
                                _server.SendResponse(package, request.Receiver);
                            }

                            //Отправка созданного чата  и друга пользователю который отправлял заявку в друзья
                            if (_server.DataBaseHelper.GetUser(request.Receiver) is User receiver)
                            {
                                package = new(Command.GetFriend, receiver);
                                _server.SendResponse(package, request.Sender);

                                package = new(Command.GetChat, chat);
                                _server.SendResponse(package, request.Sender);
                            }
                            return true;
                        }
                    case Command.RemoveFriend:
                        {
                            User user = pckg.GetValue<User>();
                            if (_server.DataBaseHelper.GetChatDialog(user.Email, UserEmail) is Chat chat)
                            {
                                if (_server.DataBaseHelper.RemoveFriend(user.Email, UserEmail))
                                {
                                    Package package = new(Command.RemoveFriend, user.Email);
                                    //Отправка удаленного друга пользователю который удалял
                                    _server.SendResponse(package, UserEmail);

                                    package = new(Command.RemoveChat, chat);
                                    _server.SendResponse(package, UserEmail);


                                    //Отправка удаленного друга пользователю который был удален
                                    package = new(Command.RemoveFriend, UserEmail);
                                    _server.SendResponse(package, user.Email);

                                    package = new(Command.RemoveChat, chat);
                                    _server.SendResponse(package, user.Email);
                                }
                            }

                            return true;
                        }
                    case Command.RemoveRequest:
                        {
                            Request request = pckg.GetValue<Request>();
                            if (_server.DataBaseHelper.RemoveRequest(request))
                            {
                                Package package = new(Command.RemoveRequest, request);
                                _server.SendResponse(package, request.Receiver);

                                package = new(Command.RemoveRequest, request);
                                _server.SendResponse(package, request.Sender);
                            }
                            return true;
                        }
                    case Command.GetMessages:
                        {
                            int id_chat = pckg.GetValue<int>();
                            if (_server.DataBaseHelper.GetMessages(id_chat) is List<Message> messages)
                            {
                                Package package = new(Command.GetMessages, messages);
                                _server.SendResponse(package, UserEmail);
                            }
                                
                            return true;
                        }
                    case Command.Message:
                        {
                            Message message = pckg.GetValue<Message>();
                            if (message != null && _server.DataBaseHelper.AddMessage(message) is Message)
                            {
                                if (!string.IsNullOrEmpty(message.FileName))
                                {
                                    string fileName = $"{message.Id_Message}.{message.FileName.Split('.')[^1]}";
                                    FileHelper.SaveFile(_pathSavedFiles, fileName, message.File);
                                    message.File = null;
                                }

                                if (_server.DataBaseHelper.GetMessage(message.Id_Message) is Message m)
                                    _server.BroadcastMessage(m, m.Chat_Id);

                            }
                            return true;
                        }
                    case Command.GetFile:
                        {
                            Message message = pckg.GetValue<Message>();
                            if (message != null)
                            {
                                Attachment attachment = GetAttachment(message.Id_Message);
                                Package package = new(Command.GetFile, attachment);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.CreateChat:
                        {
                            Chat chat = pckg.GetValue<Chat>();
                            if (chat != null && _server.DataBaseHelper.AddChat(chat) is Chat)
                            {
                                Package package = new(Command.GetChat, chat);
                                _server.SendResponse(package, UserEmail);
                            }
                                
                            return true;
                        }
                    case Command.AddUserToChat:
                        {
                            UserChatDTO userChat = pckg.GetValue<UserChatDTO>();
                            if (_server.DataBaseHelper.AddUserInChat(userChat.Id_Chat, userChat.Email))
                            {
                                Package package = new(Command.AddUserToChat, userChat.Email);
                                _server.SendResponse(package, UserEmail);
                                if (_server.DataBaseHelper.GetChat(userChat.Id_Chat) is Chat chat)
                                {
                                    package = new Package(Command.GetChat, chat);
                                    _server.SendResponse(package, userChat.Email);
                                }
                                    
                            }

                            return true;
                        }
                    case Command.RemoveUserFromChat:
                        {
                            UserChatDTO userChat = pckg.GetValue<UserChatDTO>();
                            if (_server.DataBaseHelper.GetChat(userChat.Id_Chat) is Chat chat && _server.DataBaseHelper.RemoveUserFromChat(userChat.Id_Chat, userChat.Email))
                            {
                                Package package = new(Command.RemoveUserFromChat, userChat.Email);
                                _server.SendResponse(package, chat.UserChats.SingleOrDefault(uc => uc.IsAdmin).Email);

                                package = new(Command.RemoveChat, new Chat() { Id_Chat = chat.Id_Chat, Server_Id = chat.Server_Id });
                                _server.SendResponse(package, userChat.Email);
                            }
                            return true;
                        }
                    case Command.RemoveChat:
                        {
                            int id_Chat = pckg.GetValue<int>();
                            if ((_server.DataBaseHelper.GetChat(id_Chat) is Chat chat) && (_server.DataBaseHelper.GetChatUsers(id_Chat) is List<User> users) && (_server.DataBaseHelper.RemoveChat(chat.Id_Chat)))
                            {
                                foreach (var message in chat.Messages)
                                    FileHelper.DeleteFile(message.FileName);

                                chat = new Chat() { Id_Chat = chat.Id_Chat, Server_Id = chat.Server_Id };
                                Package package = new(Command.RemoveChat, chat);
                                SendResponseToUsers(users, package);
                            }
                            return true;
                        }
                    case Command.CreateServer:
                        {
                            Server server = pckg.GetValue<Server>();
                            server.InviteCode = Guid.NewGuid().ToString();
                            server = _server.DataBaseHelper.CreateServer(server, UserEmail);
                            if (server != null)
                            {
                                if (server.File != null)
                                    FileHelper.SaveFile(_pathSavedServerIcon, $"{server.Id_Server}.{server.FileName}", server.File);

                                Package package = new(Command.CreateServer, server);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.GetChat:
                        {
                            int id_Chat = pckg.GetValue<int>();
                            if (_server.DataBaseHelper.GetChat(id_Chat) is Chat chat)
                            {
                                Package package = new(Command.GetChat, chat);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.GetServer:
                        {
                            int idServer = pckg.GetValue<int>();
                            if (_server.DataBaseHelper.GetServer(idServer, UserEmail) is Server server)
                            {
                                LoadServerIcon(server);
                                Package package = new(Command.GetServer, server);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.ChangeServer:
                        {
                            Server server = pckg.GetValue<Server>();
                            FileHelper.DeleteFile(_pathSavedServerIcon, $"{server.Id_Server}.*.*");
                            server = _server.DataBaseHelper.ChangeServer(server);
                            if (server.File != null)
                                FileHelper.SaveFile(_pathSavedServerIcon, $"{server.Id_Server}.{server.FileName}", server.File);
                            if (_server.DataBaseHelper.GetUsersServer(server.Id_Server) is IEnumerable<User> users)
                            {
                                Package package = new(Command.ChangeServer, server);
                                SendResponseToUsers(users, package);
                            }
                                

                            return true;
                        }
                    case Command.ChangeRole:
                        {
                            Role role = pckg.GetValue<Role>();
                            role = _server.DataBaseHelper.ChangeRole(role);
                            Package package = new(Command.ChangeRole, role);
                            SendResponseToUsers(role.Users, package);
                            return true;
                        }
                    case Command.RemoveRole:
                        {
                            int id = pckg.GetValue<int>();
                            if (_server.DataBaseHelper.GetRole(id) is Role removedRole && _server.DataBaseHelper.RemoveRole(id) &&
                            _server.DataBaseHelper.GetUsersServer(removedRole.Server_Id) is List<User> users)
                            {
                                Package package = new(Command.RemoveRole, removedRole);
                                SendResponseToUsers(users, package);
                            }
                                
                            return true;
                        }
                    case Command.AddUserToRole:
                        {
                            //TODO: не особо нравится переменная usersRole мб можно как то переделать?!?
                            Role role = pckg.GetValue<Role>();
                            var usersRole = role.Users;
                            if (_server.DataBaseHelper.AddUserToRole(role))
                            {
                                role = _server.DataBaseHelper.GetRole(role.Id_Role);
                                role.Users = usersRole;
                                if (role != null && _server.DataBaseHelper.GetUsersServer(role.Server_Id) is List<User> users)
                                {
                                    Package package = new(Command.AddUserToRole, role);
                                    SendResponseToUsers(users, package);
                                }
                                    
                            }
                            return true;
                        }
                    case Command.RemoveUserFromRole:
                        {
                            Tuple<string, int> tuple = pckg.GetValue<Tuple<string, int>>();
                            if (_server.DataBaseHelper.GetRole(tuple.Item2) is Role role && _server.DataBaseHelper.RemoveUserFromRole(tuple.Item1, tuple.Item2))
                            {
                                string email = null;
                                IEnumerable<User> users = _server.DataBaseHelper.GetUsersServer(role.Server_Id);
                                if (role.Name == "Everyone" && _server.DataBaseHelper.RemoveUserFromServer(role.Server_Id, tuple.Item1))
                                    email = tuple.Item1;
                                if (users != null)
                                {
                                    foreach (var user in users)
                                    {
                                        Package package;
                                        if (!string.IsNullOrEmpty(email))
                                        {
                                            package = new(Command.RemoveUserFromServer, new ServerObject(role.Server_Id, email));
                                            _server.SendResponse(package, user.Email);
                                            continue;
                                        }
                                        package = new(Command.RemoveUserFromRole, new ServerObject(role.Server_Id, tuple));
                                        _server.SendResponse(package, user.Email);
                                    }
                                }
                            }
                            return true;
                        }
                    case Command.ChangeAccessRole:
                        {
                            List<Tuple<Chat, string, Role>> tuple = pckg.GetValue<List<Tuple<Chat, string, Role>>>();
                            _server.DataBaseHelper.ChangeAccessRole(tuple);
                            if (_server.DataBaseHelper.GetUsersServer((int)tuple[0].Item1.Server_Id) is List<User> users)
                            {
                                Package package = new(Command.ChangeAccessRole, tuple);
                                SendResponseToUsers(users, package);
                            }
                                
                            return true;
                        }
                    case Command.CreateChannel:
                        {
                            Chat chat = pckg.GetValue<Chat>();
                            chat = _server.DataBaseHelper.CreateChannel(chat);
                            if (chat != null)
                            {
                                Package package = new(Command.CreateChannel, chat);
                                SendResponseToUsers(_server.DataBaseHelper.GetChatUsers(chat.Id_Chat), package);
                            }
                                
                            return true;
                        }
                    case Command.RenameChannel:
                        {
                            Chat chat = pckg.GetValue<Chat>();
                            if (_server.DataBaseHelper.RenameChannel(chat) && _server.DataBaseHelper.GetUsersServer((int)chat.Server_Id) is List<User> users)
                            {
                                Package package = new(Command.RenameChannel, chat);
                                SendResponseToUsers(users, package);
                            }   
                            return true;
                        }
                    case Command.GetProfileInfo:
                        {
                            string email = pckg.GetValue<string>();
                            if (_server.DataBaseHelper.GetRequests(email) is List<Request> requests)
                            {
                                Package package = new(Command.GetProfileInfo, requests);
                                _server.SendResponse(package, UserEmail);
                            }

                            return true;
                        }
                    case Command.GetChats:
                        {
                            string email = pckg.GetValue<string>();
                            if (_server.DataBaseHelper.GetChats(email) is List<Chat> chats)
                            {
                                Package package = new(Command.GetChats, chats);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.GetUser:
                        {
                            string email = pckg.GetValue<string>();
                            if (_server.DataBaseHelper.GetUser(email) is User user)
                            {
                                Package package = new(Command.GetUser, user);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.GetFriends:
                        {
                            string email = pckg.GetValue<string>();
                            if (_server.DataBaseHelper.GetFriends(email) is List<User> friends)
                            {
                                Package package = new(Command.GetFriends, friends);
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.JoinServer:
                        {
                            string serverCode = pckg.GetValue<string>();
                            Server server = _server.DataBaseHelper.JoinServer(serverCode, UserEmail);
                            Package package;
                            if (server != null)
                            {
                                LoadServerIcon(server);
                                package = new(Command.CreateServer, server);
                                _server.SendResponse(package, UserEmail);

                                if (_server.DataBaseHelper.GetUsersServer(server.Id_Server) is List<User> users)
                                {
                                    User newUser = users.FirstOrDefault(u => u.Email == UserEmail);
                                    users.Remove(newUser);
                                    package = new(Command.JoinServer, new ServerObject(server.Id_Server, newUser));
                                    SendResponseToUsers(users, package);
                                }

                                return true;
                            }

                            package = new(Command.Error, "Вы уже подключены к серверу");
                            _server.SendResponse(package, UserEmail);

                            return true;
                        }
                    case Command.AddRole:
                        {
                            int idServer = pckg.GetValue<int>();
                            if (_server.DataBaseHelper.AddRole(idServer) is Role role)
                            {
                                var users = _server.DataBaseHelper.GetUsersServer(role.Server_Id);
                                Package package = new(Command.AddRole, role);
                                SendResponseToUsers(users, package);
                            }

                            return true;
                        }
                    case Command.GetChatsByRole:
                        {
                            Role role = pckg.GetValue<Role>();
                            if (_server.DataBaseHelper.GetChatsByRole(role.Id_Role) is List<Chat> chats)
                            {
                                Package package = new(Command.GetChatsByRole, new ServerObject(role.Server_Id, chats));
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.Call:
                        {
                            Call call = pckg.GetValue<Call>();
                            Package package;
                            if (_server.IsConnected(call.Answerer))
                            {
                                package = new(Command.Call, call.Caller);
                                _server.SendResponse(package, call.Answerer);


                                package = new(Command.CallResponse, call.Answerer);
                                _server.SendResponse(package, UserEmail);
                                return true;
                            }
                            package = new(Command.CallResponse, "Вызываемый пользователь не в сети");
                            _server.SendResponse(package, UserEmail);

                            return true;
                        }
                    case Command.CallCancel:
                        {
                            CancelCall call = pckg.GetValue<CancelCall>();
                            if (call.Chat.IsVoiceChat)
                                _server.DataBaseHelper.RemoveUserFromChat(call.Chat.Id_Chat, UserEmail);
                            foreach (var ipUser in call.Users)
                            {
                                Package package = new(Command.CallCancel, UserEmail);
                                _server.SendResponse(package, ipUser.Email);
                            }

                            return true;
                        }
                    case Command.GetIP:
                        {
                            Call call = pckg.GetValue<Call>();
                            if (_server.IsConnected(call.Answerer))
                            {
                                Package package = new(Command.GetIP, call.Caller);
                                _server.SendResponse(package, call.Answerer);
                            }
                            return true;
                        }
                    case Command.ConnectVoiceChat:
                        {
                            GroupCall groupCall = pckg.GetValue<GroupCall>();
                            Package package;
                            if (_server.DataBaseHelper.GetVoiceChatUsers(groupCall.Chat.Id_Chat) is IEnumerable<User> users && (users is null || users.Count() < 5))
                            {
                                if(_server.DataBaseHelper.AddUserInChat(groupCall.Chat.Id_Chat, groupCall.IpUser.Email))
                                {
                                    package = new(Command.ConnectVoiceChat, groupCall);
                                    _server.SendResponse(package, UserEmail);
                                }
                                else
                                {
                                    package = new Package(Command.CallCancel, "Ошибка при добавление в голосовой чат");
                                    _server.SendResponse(package, UserEmail);
                                }
                            }
                            else
                            {
                                package = new Package(Command.CallCancel, "Все места в голосовом канале заняты");
                                _server.SendResponse(package, UserEmail);
                            }
                            return true;
                        }
                    case Command.CheckUsersInVoiceChat:
                        {
                            GroupCall groupCall = pckg.GetValue<GroupCall>();
                            if (_server.DataBaseHelper.GetVoiceChatUsers(groupCall.Chat.Id_Chat) is IEnumerable<User> users)
                            {
                                foreach(var user in users)
                                {
                                    if(user.Email != UserEmail)
                                    {
                                        Package package = new(Command.CheckUsersInVoiceChat, groupCall);
                                        _server.SendResponse(package, user.Email);
                                    }
                                }
                            }
                            return true;
                        }
                    case Command.DeleteServer:
                        {
                            int idServer = pckg.GetValue<int>();
                            if (_server.DataBaseHelper.GetUsersServer(idServer) is IEnumerable<User> users && _server.DataBaseHelper.RemoveServer(idServer))
                            {
                                FileHelper.DeleteFile(_pathSavedServerIcon, $"{idServer}.*.*");
                                Package package = new(Command.DeleteServer, idServer);
                                SendResponseToUsers(users, package);
                            }
                            return true;
                        }
                    case Command.Exit:
                        {
                            string request = pckg.GetValue<string>();

                            Package package = new(Command.Exit, "Exit");
                            _server.SendResponse(package, UserEmail);
                            return false;
                        }
                }
            }
            catch (Exception ex)
            {
                //Надо мб добавить ещё catch для разных видах ошибок(DbException, JsonCycle и тыды)
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
            return false;
        }

        private T GetRequest<T>()
        {
            string test = _reader.ReadString();
            return JsonSerializer.Deserialize<T>(test);
        }
            


        private void SendResponseToUsers(IEnumerable<User> users, Package package)
        {
            foreach (var user in users)
                _server.SendResponse(package, user.Email);
        }


        private Attachment GetAttachment(int idMessage)
        {
            if (_server.DataBaseHelper.GetMessage(idMessage) is Message message)
            {
                string fileName = $"{message.Id_Message}.{message.FileName.Split('.')[^1]}";
                var file = FileHelper.LoadFile(_pathSavedFiles, fileName);
                Attachment attachment = new(message.FileName, file);
                return attachment;
            }
            return null;
        }

        /// <summary>
        /// Закрытие клиента на сервере
        /// </summary>
        protected internal void Close()
        {
            _server.RemoveConnection(UserEmail);
            if (Stream != null)
                Stream.Close();
            if (_client != null)
                _client.Close();
            if (_reader != null)
                _reader.Close();
        }
    }
}
