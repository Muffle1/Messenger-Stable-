using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger
{
    public delegate void ServerCmdEventHandler(object sender, ServerCmdEventArgs e);

    public static class ServerContext
    {
        public static event Action ExitCmdReceived;
        public static event ServerCmdEventHandler EnterRequestCmdReceived;
        public static event ServerCmdEventHandler RemoveRequestCmdReceived;
        public static event ServerCmdEventHandler GetFriendCmdReceived;
        public static event ServerCmdEventHandler RemoveFriendCmdReceived;
        public static event ServerCmdEventHandler GetChatCmdReceived;
        public static event ServerCmdEventHandler MessageCmdReceived;
        public static event ServerCmdEventHandler RemoveChatCmdReceived;
        public static event ServerCmdEventHandler GetSearchedUsersCmdReceived;
        public static event ServerCmdEventHandler SendFriendRequestCmdReceived;
        public static event ServerCmdEventHandler GetMessagesCmdReceived;
        public static event ServerCmdEventHandler GetFileCmdReceived;
        public static event ServerCmdEventHandler GetChatsCmdReceived;
        public static event ServerCmdEventHandler AddUserToChatCmdReceived;
        public static event ServerCmdEventHandler RemoveUserFromChatCmdReceived;
        public static event ServerCmdEventHandler CreateServerCmdReceived;
        public static event ServerCmdEventHandler JoinServerCmdReceived;
        public static event ServerCmdEventHandler GetServerCmdReceived;
        public static event ServerCmdEventHandler ChangeServerCmdReceived;
        public static event ServerCmdEventHandler ChangeRoleCmdReceived;
        public static event ServerCmdEventHandler RemoveRoleCmdReceived;
        public static event ServerCmdEventHandler ChangeAccessRoleCmdReceived;
        public static event ServerCmdEventHandler AddUserToRoleCmdReceived;
        public static event ServerCmdEventHandler RemoveUserFromRoleCmdReceived;
        public static event ServerCmdEventHandler CreateChannelCmdReceived;
        public static event ServerCmdEventHandler RenameChannelCmdReceived;
        public static event ServerCmdEventHandler GetProfileInfoCmdReceived;
        public static event ServerCmdEventHandler GetUserCmdReceived;
        public static event ServerCmdEventHandler GetFriendsCmdReceived;
        public static event ServerCmdEventHandler AddRoleCmdReceived;
        public static event ServerCmdEventHandler GetChatsByRoleCmdReceived;
        public static event ServerCmdEventHandler DeleteServerCmdReceived;
        public static event ServerCmdEventHandler RemoveUserFromServerCmdReceived;
        public static event ServerCmdEventHandler ErrorCmdReceived;
        public static event ServerCmdEventHandler CallCmdReceived;
        public static event ServerCmdEventHandler CallResponseCmdReceived;
        public static event ServerCmdEventHandler CallCancelCmdReceived;
        public static event ServerCmdEventHandler GetIPCmdReceived;
        public static event ServerCmdEventHandler ConnectVoiceChatCmdReceived;
        public static event ServerCmdEventHandler CheckUsersInVoiceChatCmdReceived;

        public static string Server { get; set; }
        public static string UserEmail { get; private set; }

        private static readonly int _port = 8005;
        private static TcpClient _client;
        private static NetworkStream _stream;
        private static BinaryWriter _writer;
        private static BinaryReader _reader;

        public static void SendRequest(Package package)
        {
            try
            {
                string message = JsonSerializer.Serialize(package);
                _writer.Write(message);
                _writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                OnExit();
                Disconnect();
            }
        }

        private static void RaiseEvent(ServerCmdEventHandler eventHandler, object param) =>
            eventHandler?.Invoke(null, new ServerCmdEventArgs(param));

        /// <summary>
        /// Получение ответа с сервера
        /// </summary>
        /// <param name="token">Токен для отмена выполнения метода</param>
        public static void ReadServerStream(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Package pckg = GetResponse<Package>();

                    switch (pckg.Command)
                    {
                        case Command.Notify:
                            {
                                Request request = pckg.GetValue<Request>();
                                RaiseEvent(EnterRequestCmdReceived, request);
                                break;
                            }
                        case Command.RemoveRequest:
                            {
                                Request request = pckg.GetValue<Request>();
                                RaiseEvent(RemoveRequestCmdReceived, request);
                                break;
                            }
                        case Command.GetFriend:
                            {
                                User user = pckg.GetValue<User>();
                                RaiseEvent(GetFriendCmdReceived, user);
                                break;
                            }
                        case Command.RemoveFriend:
                            {
                                string email = pckg.GetValue<string>();
                                RaiseEvent(RemoveFriendCmdReceived, email);
                                break;
                            }
                        case Command.GetChat:
                            {
                                Chat chat = pckg.GetValue<Chat>();
                                RaiseEvent(GetChatCmdReceived, chat);
                                break;
                            }
                        case Command.Message:
                            {
                                Message message = pckg.GetValue<Message>();
                                RaiseEvent(MessageCmdReceived, message);
                                break;
                            }
                        case Command.RemoveChat:
                            {
                                Chat chat = pckg.GetValue<Chat>();
                                RaiseEvent(RemoveChatCmdReceived, chat);
                                break;
                            }
                        case Command.GetSearchedUsers:
                            {
                                User[] users = pckg.GetValue<User[]>();
                                RaiseEvent(GetSearchedUsersCmdReceived, users);
                                break;
                            }
                        case Command.SendFriendRequest:
                            {
                                Request request = pckg.GetValue<Request>();
                                RaiseEvent(SendFriendRequestCmdReceived, request);
                                break;
                            }
                        case Command.GetMessages:
                            {
                                List<Message> messages = pckg.GetValue<List<Message>>();
                                RaiseEvent(GetMessagesCmdReceived, messages);
                                break;
                            }
                        case Command.GetFile:
                            {
                                Attachment attachment = pckg.GetValue<Attachment>();
                                RaiseEvent(GetFileCmdReceived, attachment);
                                break;
                            }
                        case Command.AddUserToChat:
                            {
                                string email = pckg.GetValue<string>();
                                RaiseEvent(AddUserToChatCmdReceived, email);
                                break;
                            }
                        case Command.RemoveUserFromChat:
                            {
                                string email = pckg.GetValue<string>();
                                RaiseEvent(RemoveUserFromChatCmdReceived, email);
                                break;
                            }
                        case Command.CreateServer:
                            {
                                Server server = pckg.GetValue<Server>();
                                RaiseEvent(CreateServerCmdReceived, server);
                                break;
                            }
                        case Command.JoinServer:
                            {
                                ServerObject serverObject = pckg.GetValue<ServerObject>();
                                serverObject.Parameter = ToObject<User>(serverObject.Parameter);
                                RaiseEvent(JoinServerCmdReceived, serverObject);
                                break;
                            }
                        case Command.GetServer:
                            {
                                Server server = pckg.GetValue<Server>();
                                RaiseEvent(GetServerCmdReceived, server);
                                break;
                            }
                        case Command.ChangeServer:
                            {
                                Server server = pckg.GetValue<Server>();
                                RaiseEvent(ChangeServerCmdReceived, server);
                                break;
                            }
                        case Command.ChangeRole:
                            {
                                Role role = pckg.GetValue<Role>();
                                RaiseEvent(ChangeRoleCmdReceived, role);
                                break;
                            }
                        case Command.RemoveRole:
                            {
                                Role role = pckg.GetValue<Role>();
                                RaiseEvent(RemoveRoleCmdReceived, role);
                                break;
                            }
                        case Command.ChangeAccessRole:
                            {
                                List<Tuple<Chat, string, Role>> tuple = pckg.GetValue<List<Tuple<Chat, string, Role>>>();
                                RaiseEvent(ChangeAccessRoleCmdReceived, tuple);
                                break;
                            }
                        case Command.AddUserToRole:
                            {
                                Role role = pckg.GetValue<Role>();
                                RaiseEvent(AddUserToRoleCmdReceived, role);
                                break;
                            }
                        case Command.RemoveUserFromRole:
                            {
                                ServerObject serverObject = pckg.GetValue<ServerObject>();
                                serverObject.Parameter = ToObject<Tuple<string, int>>(serverObject.Parameter);
                                RaiseEvent(RemoveUserFromRoleCmdReceived, serverObject);
                                break;
                            }
                        case Command.CreateChannel:
                            {
                                Chat chat = pckg.GetValue<Chat>();
                                RaiseEvent(CreateChannelCmdReceived, chat);
                                break;
                            }
                        case Command.RenameChannel:
                            {
                                Chat chat = pckg.GetValue<Chat>();
                                RaiseEvent(RenameChannelCmdReceived, chat);
                                break;
                            }
                        case Command.GetProfileInfo:
                            {
                                List<Request> requests = pckg.GetValue<List<Request>>();
                                RaiseEvent(GetProfileInfoCmdReceived, requests);
                                break;
                            }
                        case Command.GetChats:
                            {
                                List<Chat> chats = pckg.GetValue<List<Chat>>();
                                RaiseEvent(GetChatsCmdReceived, chats);
                                break;
                            }
                        case Command.GetUser:
                            {
                                User user = pckg.GetValue<User>();
                                RaiseEvent(GetUserCmdReceived, user);
                                break;
                            }
                        case Command.GetFriends:
                            {
                                List<User> friends = pckg.GetValue<List<User>>();
                                RaiseEvent(GetFriendsCmdReceived, friends);
                                break;
                            }
                        case Command.AddRole:
                            {
                                Role role = pckg.GetValue<Role>();
                                RaiseEvent(AddRoleCmdReceived, role);
                                break;
                            }
                        case Command.GetChatsByRole:
                            {
                                ServerObject serverObject = pckg.GetValue<ServerObject>();
                                serverObject.Parameter = ToObject<List<Chat>>(serverObject.Parameter);
                                RaiseEvent(GetChatsByRoleCmdReceived, serverObject);
                                break;
                            }
                        case Command.DeleteServer:
                            {
                                int idServer = pckg.GetValue<int>();
                                RaiseEvent(DeleteServerCmdReceived, idServer);
                                break;
                            }
                        case Command.RemoveUserFromServer:
                            {
                                ServerObject serverObject = pckg.GetValue<ServerObject>();
                                serverObject.Parameter = ToObject<string>(serverObject.Parameter);
                                RaiseEvent(RemoveUserFromServerCmdReceived, serverObject);
                                break;
                            }
                        case Command.Error:
                            {
                                string error = pckg.GetValue<string>();
                                RaiseEvent(ErrorCmdReceived, error);
                                break;
                            }
                        case Command.Call:
                            {
                                IPUser ipUser = pckg.GetValue<IPUser>();
                                RaiseEvent(CallCmdReceived, ipUser);
                                break;
                            }
                        case Command.CallResponse:
                            {
                                string response = pckg.GetValue<string>();
                                RaiseEvent(CallResponseCmdReceived, response);
                                break;
                            }
                        case Command.CallCancel:
                            {
                                string email = pckg.GetValue<string>();
                                RaiseEvent(CallCancelCmdReceived, email);
                                break;
                            }
                        case Command.GetIP:
                            {
                                IPUser ipUser = pckg.GetValue<IPUser>();
                                RaiseEvent(GetIPCmdReceived, ipUser);
                                break;
                            }
                        case Command.ConnectVoiceChat:
                            {
                                GroupCall groupCall = pckg.GetValue<GroupCall>();
                                RaiseEvent(ConnectVoiceChatCmdReceived, groupCall);
                                break;
                            }
                        case Command.CheckUsersInVoiceChat:
                            {
                                GroupCall group = pckg.GetValue<GroupCall>();
                                RaiseEvent(CheckUsersInVoiceChatCmdReceived, group);
                                break;
                            }
                        case Command.Exit:
                            {
                                break;
                            }
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                OnExit();
            }
            finally
            {
                Disconnect();
            }
        }

        public static T ToObject<T>(object element)
        {
            if (element is JsonElement)
                return JsonSerializer.Deserialize<T>(((JsonElement)element).GetString());
            else if (element is JObject) return ((JObject)element).ToObject<T>();
            else if (element is JArray) return ((JArray)element).ToObject<T>();
            else return (T)element;

        }

        private static T GetResponse<T>() =>
            JsonSerializer.Deserialize<T>(_reader.ReadString());

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Успешная ли авторизация</returns>
        public static User Authorization(User user)
        {
            if ((_client == null) || ((_client != null) && (!_client.Connected)))
            {
                if (!Connect())
                    return null;
            }

            Package package = new(Command.Enter, user);
            SendRequest(package);

            package = GetResponse<Package>();
            User response = package.GetValue<User>();
            if ((package.Command == Command.Enter) && (response != null))
            {
                UserEmail = response.Email;
                return response;
            }

            Disconnect();
            return null;
        }

        /// <summary>
        /// Регистрация
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Успешная ли регистрация</returns>
        public static bool Registration(User user)
        {
            if ((_client == null) || ((_client != null) && (!_client.Connected)))
            {
                if (!Connect())
                    return false;
            }

            Package package = new(Command.Register, user);
            SendRequest(package);

            package = GetResponse<Package>();
            string response = package.GetValue<string>();
            Disconnect();
            if ((package.Command == Command.Register) && (response == "Approved"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Проверка на подключения пользователя к серверу
        /// </summary>
        /// <returns>Подключен ли пользователь</returns>
        public static bool IsConnected()
        {
            if (_client != null)
                return _client.Connected;
            else
                return false;
        }

        /// <summary>
        /// Подключение к серверу
        /// </summary>
        /// <returns>Успешное ли подключение</returns>
        private static bool Connect()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(Server, _port);
                _stream = _client.GetStream();
                _reader = new BinaryReader(_stream);
                _writer = new BinaryWriter(_stream);
                return true;
            }
            catch
            {
                OnExit();
                Disconnect();
                return false;
            }
        }

        /// <summary>
        /// Отключение от сервера
        /// </summary>
        private static void Disconnect()
        {
            ExitCmdReceived?.Invoke();

            if (_stream != null)
                _stream.Dispose();
            if (_client != null)
                _client.Close();
            if (_writer != null)
                _writer.Close();
            if (_reader != null)
                _reader.Close();
        }

        /// <summary>
        /// Вызов события выхода в главное окно
        /// </summary>
        private static void OnExit()
        {
            try
            {
                _writer?.Write((int)Command.Exit);
                _writer?.Write(JsonSerializer.Serialize("Exit"));
                _writer?.Flush();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                //Disconnect();
            }
        }
    }
}
