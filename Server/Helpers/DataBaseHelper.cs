using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class DataBaseHelper
    {
        private DbContextOptions _options;

        public DataBaseHelper(DbContextOptions options)
        {
            _options = options;
        }

        public User AddUser(User user)
        {
            try
            {
                using DataBaseContext db = new(_options);
                if (!db.Users.Any(u => (u.Email == user.Email) || (u.Login == user.Login)))
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return user;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении пользователя: {ex.Message}");
            }

            return null;
        }

        public string GetPasswordUser(string email)
        {
            try
            {
                using DataBaseContext db = new(_options);
                return db.Users.SingleOrDefault(u => u.Email == email)?.Password;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пароля: {ex.Message}");
            }
            return null;
        }

        public User GetUser(string email)
        {
            try
            {
                using DataBaseContext db = new(_options);
                User user = db.Users
                            .Select(f => new User()
                            {
                                Email = f.Email,
                                Name = f.Name,
                                SurName = f.SurName,
                                PhoneNumber = f.PhoneNumber,
                                Login = f.Login,
                                Birthday = f.Birthday
                            })
                            .FirstOrDefault(u => u.Email == email);

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пользователя: {ex.Message}");
            }
            return null;
        }

        public User GetUser(string email, string password)
        {
            try
            {
                using DataBaseContext db = new(_options);
                User user = db.Users
                              .Include(u => u.Settings)
                              .FirstOrDefault(u => (u.Email == email) && (u.Password == password));

                if (user != null)
                {
                    user.Servers = db.Servers.Where(s => s.Users.Any(u => u.Email == user.Email))
                                             .Select(s => new Server()
                                             {
                                                 Id_Server = s.Id_Server,
                                                 Name = s.Name,
                                                 File = s.File,
                                                 FileName = s.FileName
                                             })
                                             .ToList();
                    // user.Roles = db.Roles.Where(r => r.Users.Any(u => u.Email == user.Email)).ToList();
                }
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пользователя: {ex.Message}");
            }
            return null;
        }

        public User[] GetSearchUsers(string search, string currentUserEmail)
        {
            try
            {
                using DataBaseContext db = new(_options);
                User[] users = db.Users.Where(u => (u.Email.Contains(search) || u.Login.Contains(search))
                                      && u.Email != currentUserEmail)
                               .Select(u => new User
                               {
                                   Email = u.Email,
                                   Login = u.Login
                               }).ToArray();
                if (GetFriends(currentUserEmail) is List<User> friends)
                    return users.Where(u => !friends.Any(f => f.Email == u.Email)).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске пользователей: {ex.Message}");
            }
            return null;

        }

        public bool SaveChangeUser(User user)
        {
            try
            {
                using DataBaseContext db = new(_options);
                db.Users.Attach(user);
                db.Entry(user).State = EntityState.Modified;
                db.Entry(user.Settings).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при изменении пользователя: {ex.Message}");
            }
            return false;
        }

        public Request AddRequest(Request request)
        {
            try
            {
                using DataBaseContext db = new(_options);
                if (!db.Requests.Any(r => (((r.Sender == request.Sender) && (r.Receiver == request.Receiver))
                    || ((r.Sender == request.Receiver) && (r.Receiver == request.Sender)))) &&
                    !db.Friendships.Any(f => (f.Friend1Email == request.UserReceiver.Email &&
                    f.Friend2Email == request.UserSender.Email) || (f.Friend2Email == request.UserReceiver.Email && f.Friend1Email == request.UserSender.Email)))
                {
                    request.UserSender = db.Users.FirstOrDefault(u => u.Email == request.UserSender.Email);
                    request.UserReceiver = db.Users.FirstOrDefault(u => u.Email == request.UserReceiver.Email);
                    db.Requests.Add(request);
                    db.SaveChanges();
                    request = db.Requests.Select(r => new Request()
                    {
                        Id_Request = r.Id_Request,
                        Sender = r.Sender,
                        Receiver = r.Receiver
                    }).FirstOrDefault(r => r.Id_Request == request.Id_Request);
                    return request;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении заявки: {ex.Message}");
            }
            return null;
        }

        public Chat CreateFriendship(Request request)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Friendship friendship = new(request.Sender, request.Receiver);
                db.Friendships.Add(friendship);
                db.Entry(request).State = EntityState.Deleted;
                db.SaveChanges();
                Chat chat = new();
                if (GetUser(request.Sender) is User sender && GetUser(request.Receiver) is User receiver)
                {
                    User[] users = new User[] { sender, receiver };
                    return CreateDialog(chat, users, db);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании friendship: {ex.Message}");
            }
            return null;
        }

        public bool RemoveFriend(string emailFriend, string emailUser)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Friendship friendship = db.Friendships.FirstOrDefault(f => ((f.Friend1Email == emailUser) && (f.Friend2Email == emailFriend)) || ((f.Friend2Email == emailUser) && (f.Friend1Email == emailFriend)));
                if (GetChatDialog(emailFriend, emailUser) is Chat chat)
                {
                    db.Chats.Remove(chat);
                    db.Friendships.Remove(friendship);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении друга: {ex.Message}");
            }
            return false;
        }

        public Chat GetChatDialog(string emailFriend, string emailUser)
        {
            try
            {
                using DataBaseContext db = new(_options);
                return db.Chats.Include(c => c.UserChats).FirstOrDefault(c => (c.Users.Any(u => u.Email == emailFriend)) && (c.Users.Any(u => u.Email == emailUser) && (c.Name == null)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении диалога: {ex.Message}");
            }
            return null;
        }


        public bool RemoveRequest(Request request)
        {
            try
            {
                using DataBaseContext db = new(_options);
                db.Requests.Attach(request);
                db.Requests.Remove(request);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении заявки: {ex.Message}");
            }
            return false;
        }

        public List<Message> GetMessages(int id_chat)
        {
            try
            {
                using DataBaseContext db = new(_options);
                List<Message> messages = db.Messages.Include(m => m.From).Where(m => m.Chat.Id_Chat == id_chat).ToList();

                foreach (var message in messages)
                    message.From.Password = null;
                return messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении сообщений: {ex.Message}");
            }
            return null;
        }

        public Message AddMessage(Message message)
        {
            try
            {
                using DataBaseContext db = new(_options);
                db.Messages.Add(message);
                db.SaveChanges();
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении сообщения: {ex.Message}");
            }
            return null;
        }

        public Message GetMessage(int id)
        {
            try
            {
                using DataBaseContext db = new(_options);
                return db.Messages.Include(m => m.From).FirstOrDefault(m => m.Id_Message == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении сообщения: {ex.Message}");
            }
            return null;
        }

        public Chat AddChat(Chat chat)
        {
            try
            {
                using DataBaseContext db = new(_options);
                db.Chats.Add(chat);
                db.SaveChanges();
                return chat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении чата: {ex.Message}");
            }
            return null;
        }

        public Chat GetChat(int id)
        {
            try
            {
                using DataBaseContext db = new(_options);
                return db.Chats.Include(c => c.Users).Include(c => c.Messages.Where(m => string.IsNullOrEmpty(m.FileName))).FirstOrDefault(c => c.Id_Chat == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении чата: {ex.Message}");
            }
            return null;
        }

        //public  Chat GetChatToRemove(int id)
        //{
        //    try
        //    {
        //        using DataBaseContext db = new();
        //        return db.Chats.Include(c => c.Users).Include(c => c.Messages.Where(m => m.File != null)).FirstOrDefault(c => c.Id_Chat == id);
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка при получении чата для добавления: {ex.Message}");
        //    }
        //    return null;
        //}

        public bool AddUserInChat(int id_Chat, string email)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Chat chat = db.Chats.Include(c => c.Users).FirstOrDefault(c => c.Id_Chat == id_Chat);
                User user = db.Users.FirstOrDefault(u => u.Email == email);
                chat.Users.Add(user);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении пользователя к чату: {ex.Message}");
            }
            return false;
        }

        private Chat CreateDialog(Chat chat, IEnumerable<User> users, DataBaseContext db)
        {
            try
            {
                db.Chats.Add(chat);
                db.SaveChanges();

                foreach (User user in users)
                    db.UserChats.Add(new UserChat() { Chat_Id = chat.Id_Chat, Email = user.Email, IsAdmin = false });
                db.SaveChanges();

                chat = db.Chats.Include(c => c.Users).FirstOrDefault(c => c.Id_Chat == chat.Id_Chat);

                return chat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании диалога: {ex.Message}");
            }
            return null;
        }

        public bool RemoveUserFromChat(int id_Chat, string email)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Chat chat = db.Chats.Include(c => c.Users).FirstOrDefault(c => c.Id_Chat == id_Chat);
                chat.Users.Remove(chat.Users.FirstOrDefault(u => u.Email == email));
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя из беседы: {ex.Message}");
            }
            return false;
        }

        public bool RemoveChat(int id)
        {
            try
            {
                using DataBaseContext db = new(_options);
                db.Chats.Remove(db.Chats.Include(c => c.Users).FirstOrDefault(c => c.Id_Chat == id));
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении чата: {ex.Message}");
            }
            return false;
        }

        public List<User> GetChatUsers(int id)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Chat chat = db.Chats.FirstOrDefault(x => x.Id_Chat == id);

                if (chat.Server_Id == null)
                    return db.Chats.Include(c => c.Users).FirstOrDefault(x => x.Id_Chat == id).Users.Select(u => new User() { Email = u.Email }).ToList();

                chat = db.Chats.Include(c => c.Roles).ThenInclude(r => r.Users).FirstOrDefault(x => x.Id_Chat == id);
                List<User> users = new();

                foreach (var role in chat.Roles)
                    users.AddRange(role.Users);

                return users.GroupBy(u => u.Email).Select(g => g.First()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при пользователей чатов: {ex.Message}");
            }
            return null;
        }

        public Server CreateServer(Server server, string userEmail)
        {
            try
            {
                using DataBaseContext db = new(_options);
                db.Servers.Add(server);
                server.Users.Add(db.Users.FirstOrDefault(u => u.Email == userEmail));
                db.SaveChanges();
                User user = db.Users.Include(u => u.Roles).FirstOrDefault(u => u.Email == userEmail);
                user.Roles.AddRange(db.Roles.Where(r => r.Server_Id == server.Id_Server));
                db.SaveChanges();
                return server;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании сервера: {ex.Message}");
            }
            return null;
        }

        public Server GetServer(int idServer, string userEmail)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Server server = db.Servers.FirstOrDefault(u => u.Id_Server == idServer);
                List<Chat> chats = db.Chats.Include(c => c.Roles)
                                           .Select(c => new Chat()
                                           {
                                               Id_Chat = c.Id_Chat,
                                               IsVoiceChat = c.IsVoiceChat,
                                               Server_Id = c.Server_Id,
                                               Name = c.Name,
                                               Roles = c.Roles.Select(r => new Role()
                                               {
                                                   Id_Role = r.Id_Role,
                                                   Users = r.Users.Select(u => new User() { Email = u.Email }).ToList(),
                                                   Server_Id = r.Server_Id
                                               })
                                                              .ToList()
                                           })
                                           .Where(c => (c.Server_Id == idServer)
                                                        && (c.Roles.Any(r => r.Users.Any(u => u.Email == userEmail))))
                                           .ToList();

                List<Role> roles = db.Roles.Where(r => r.Server_Id == server.Id_Server)
                                           .Select(r => new Role()
                                           {
                                               Id_Role = r.Id_Role,
                                               Name = r.Name,
                                               Server_Id = r.Server_Id
                                           })
                                           .ToList();

                server.Chats = new List<Chat>(chats);
                server.Chats.ForEach(c => c.Roles = c.Roles.Select(r => new Role() { Id_Role = r.Id_Role, Server_Id = r.Server_Id }).ToList());

                List<User> users = db.Servers.Include(s => s.Users)
                                             .ThenInclude(u => u.Roles.Where(r => r.Server_Id == idServer))
                                             .FirstOrDefault(s => s.Id_Server == idServer)
                                             .Users;

                users = users.Select(u => new User()
                {
                    Name = u.Name,
                    SurName = u.SurName,
                    Email = u.Email,
                    Roles = u.Roles.Select(r => new Role()
                    {
                        Id_Role = r.Id_Role,
                        Name = r.Name,
                        Server_Id = r.Server_Id
                    })
                    .ToList()
                })
                .ToList();

                server.Users = new List<User>(users);
                server.Roles = new List<Role>(roles);

                return new Server()
                {
                    Id_Server = server.Id_Server,
                    Name = server.Name,
                    File = server.File,
                    FileName = server.FileName,
                    InviteCode = server.InviteCode,
                    Users = new List<User>(server.Users),
                    Roles = new List<Role>(server.Roles),
                    Chats = new List<Chat>(server.Chats)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении сервера: {ex.Message}");
            }
            return null;
        }

        public List<Chat> GetChats(string email)
        {
            try
            {
                using DataBaseContext db = new(_options);

                List<Chat> chats = db.Chats.Include(c => c.Users).Where(c => (c.Users.Any(u => u.Email == email)) && (c.Server_Id == null)).ToList();

                foreach (var chat in chats)
                    chat.Users = chat.Users.Select(u => new User()
                    {
                        Email = u.Email,
                        Name = u.Name,
                        SurName = u.SurName
                    }).ToList();

                return chats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении чатов: {ex.Message}");
            }
            return null;
        }

        public Chat CreateChannel(Chat chat)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Chat newChannel = new()
                {
                    Name = chat.Name,
                    IsVoiceChat = chat.IsVoiceChat,
                    Server_Id = chat.Server_Id
                };

                Role[] roles = db.Roles.Include(r => r.Users)
                                       .Where(r => (r.Server_Id == chat.Server_Id) && ((r.Name == "Everyone") || (r.Name == "Admin")))
                                       .ToArray();
                newChannel.Roles = new(roles);
                db.Chats.Add(newChannel);
                db.SaveChanges();

                chat = new Chat()
                {
                    Id_Chat = newChannel.Id_Chat,
                    Name = newChannel.Name,
                    IsVoiceChat = newChannel.IsVoiceChat,
                    Server_Id = newChannel.Server_Id,
                    Roles = new List<Role>(newChannel.Roles.Select(r => new Role() { Id_Role = r.Id_Role }))
                };

                return chat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании канала: {ex.Message}");
            }
            return null;
        }

        public bool RenameChannel(Chat chat)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Chat oldChat = db.Chats.FirstOrDefault(c => c.Id_Chat == chat.Id_Chat);
                oldChat.Name = chat.Name;
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при переименовании канала: {ex.Message}");
            }
            return false;
        }
        public List<User> GetFriends(string email)
        {
            try
            {
                using DataBaseContext db = new(_options);
                List<Friendship> friendships = db.Friendships.Where(f => (f.Friend1Email == email) || (f.Friend2Email == email)).ToList();
                List<User> users = friendships.Select(f => new User()
                {
                    Email = f.Friend1Email == email ? f.Friend2Email : f.Friend1Email
                }).ToList();

                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении друзей: {ex.Message}");
            }
            return null;
        }

        public List<Request> GetRequests(string email)
        {
            try
            {
                using DataBaseContext db = new(_options);
                List<Request> requests = db.Requests.Where(r => (r.Receiver == email) || (r.Sender == email)).ToList();

                requests = requests.Select(r => new Request()
                {
                    Id_Request = r.Id_Request,
                    Receiver = r.Receiver,
                    Sender = r.Sender
                }).ToList();
                return requests;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получения заявок: {ex.Message}");
            }
            return null;
        }

        public Server JoinServer(string serverCode, string userEmail)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Server server = db.Servers.Include(s => s.Users).FirstOrDefault(s => s.InviteCode == serverCode);
                User user = db.Users.Include(u => u.Roles).FirstOrDefault(u => (u.Email == userEmail) && (!u.Servers.Any(s => s.Id_Server == server.Id_Server)));

                if (user != null)
                {
                    server.Users.Add(user);
                    user.Roles.Add(db.Roles.FirstOrDefault(r => (r.Name == "Everyone") && (r.Server_Id == server.Id_Server)));
                    db.SaveChanges();

                    server = new Server()
                    {
                        Id_Server = server.Id_Server,
                        Name = server.Name,
                        FileName = server.FileName,
                        InviteCode = server.InviteCode
                    };
                    return server;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при присоединении на сервер: {ex.Message}");
            }
            return null;
        }

        public Role AddRole(int idServer)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Role role = new()
                {
                    Name = "DefaultName",
                    Server_Id = idServer
                };
                db.Roles.Add(role);
                db.SaveChanges();
                return role;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении роли: {ex.Message}");
            }
            return new Role();
        }

        public Server ChangeServer(Server server)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Server dbServer = db.Servers.FirstOrDefault(s => s.Id_Server == server.Id_Server);
                if (dbServer != null)
                {
                    dbServer.Name = server.Name;
                    dbServer.FileName = server.FileName;
                    db.SaveChanges();
                }
                return new Server()
                {
                    Name = dbServer.Name,
                    Id_Server = dbServer.Id_Server,
                    FileName = dbServer.FileName,
                    File = server.File
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при изменении сервера: {ex.Message}");
            }
            return new Server();
        }

        public IEnumerable<User> GetUsersServer(int ServerId)
        {
            try
            {
                using DataBaseContext db = new(_options);
                return db.Servers.Include(s => s.Users).ThenInclude(u => u.Roles).FirstOrDefault(s => s.Id_Server == ServerId).Users.Select(u => new User()
                {
                    Name = u.Name,
                    SurName = u.SurName,
                    Email = u.Email,
                    Roles = u.Roles.Select(r => new Role()
                    {
                        Id_Role = r.Id_Role,
                        Name = r.Name,
                        Server_Id = r.Server_Id
                    })
                    .ToList()
                })
                .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пользователей сервера: {ex.Message}");
            }
            return null;
        }

        public Role ChangeRole(Role role)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Role dbRole = db.Roles.FirstOrDefault(s => s.Id_Role == role.Id_Role);
                if (dbRole != null)
                {
                    dbRole.Name = role.Name;
                    db.SaveChanges();
                }
                Server server = db.Servers.FirstOrDefault(s => s.Roles.Any(x => x.Id_Role == dbRole.Id_Role));
                dbRole.Users = db.Users.Where(u => u.Servers.Any(s => s.Id_Server == server.Id_Server) 
                                                    && u.Roles.Any(r => r.Name == "Admin" && r.Server_Id == server.Id_Server))
                                       .Select(u => new User() { Email = u.Email })
                                       .ToList();
                return dbRole;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка изменении роли: {ex.Message}");
            }
            return new Role();
        }

        public Role GetRole(int id)
        {
            try
            {
                using DataBaseContext db = new(_options);
                return db.Roles.Include(r => r.Users).FirstOrDefault(r => r.Id_Role == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении роли: {ex.Message}");
            }
            return null;
        }

        public bool RemoveRole(int id)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Role dbRole = db.Roles.Include(r => r.Users).FirstOrDefault(r => r.Id_Role == id);
                db.Remove(dbRole);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении роли: {ex.Message}");
            }
            return false;
        }

        public void ChangeAccessRole(List<Tuple<Chat, string, Role>> tuples)
        {
            try
            {
                using DataBaseContext db = new(_options);
                if (db.Roles.Include(r => r.Chats).FirstOrDefault(r => r.Id_Role == tuples[0].Item3.Id_Role) is Role dbRole)
                {
                    foreach (var t in tuples)
                    {
                        Chat chat = db.Chats.FirstOrDefault(x => x.Id_Chat == t.Item1.Id_Chat);
                        if (t.Item2 == "add")
                            dbRole.Chats.Add(chat);
                        else dbRole.Chats.Remove(chat);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при изменении доступа роли: {ex.Message}");
            }
        }

        public bool AddUserToRole(Role role)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Role dbRole = db.Roles.Include(r => r.Chats).Include(r => r.Users).FirstOrDefault(r => r.Id_Role == role.Id_Role);
                foreach (var email in role.Users.Select(u => u.Email))
                {
                    User dbUser = db.Users.FirstOrDefault(u => u.Email == email);
                    dbRole.Users.Add(dbUser);
                }
                db.SaveChanges();
                return true;
                //return new Role()
                //{
                //    Id_Role = dbRole.Id_Role,
                //    Name = dbRole.Name,
                //    Server_Id = dbRole.Server_Id,
                //    Users = role.Users
                //};
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении пользователя к роли: {ex.Message}");
            }
            return false;
        }

        public bool RemoveUserFromRole(string email, int idRole)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Role dbRole = db.Roles.Include(r => r.Users).FirstOrDefault(r => r.Id_Role == idRole);
                dbRole.Users.Remove(dbRole.Users.FirstOrDefault(u => u.Email == email));
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя из роли: {ex.Message}");
            }
            return false;
        }

        public List<Chat> GetChatsByRole(int idRole)
        {
            List<Chat> chats = new();
            try
            {
                using DataBaseContext db = new(_options);
                chats = db.Roles.Include(r => r.Chats)
                                           .ThenInclude(c => c.Roles)
                                           .ThenInclude(r => r.Users)
                                           .FirstOrDefault(r => r.Id_Role == idRole)
                                           .Chats
                                           .Select(c => new Chat()
                                           {
                                               Id_Chat = c.Id_Chat,
                                               IsVoiceChat = c.IsVoiceChat,
                                               Server_Id = c.Server_Id,
                                               Name = c.Name,
                                               Roles = c.Roles.Select(r => new Role()
                                               {
                                                   Id_Role = r.Id_Role,
                                                   Users = r.Users.Select(u => new User() { Email = u.Email }).ToList(),
                                                   Server_Id = r.Server_Id
                                               }).ToList()
                                           })
                                           .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении чатов по ролям: {ex.Message}");
            }
            return chats;
        }

        public bool RemoveServer(int idServer)
        {
            try
            {
                using DataBaseContext db = new(_options);
                db.Servers.Remove(db.Servers.FirstOrDefault(s => s.Id_Server == idServer));
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении сервера: {ex.Message}");
            }
            return false;
        }

        public bool RemoveUserFromServer(int idServer, string email)
        {
            try
            {
                using DataBaseContext db = new(_options);
                if (db.Servers.Include(s => s.Users).Include(s => s.Roles).ThenInclude(r => r.Users).FirstOrDefault(s => s.Id_Server == idServer) is Server server)
                {
                    server.Users.Remove(server.Users.FirstOrDefault(u => u.Email == email));

                    foreach (var role in server.Roles)
                    {
                        if (role.Users.FirstOrDefault(u => u.Email == email) is User user)
                            role.Users.Remove(user);
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пользователя с сервера: {ex.Message}");
            }
            return false;
        }

        public IEnumerable<User> GetVoiceChatUsers(int idChat)
        {
            try
            {
                using DataBaseContext db = new(_options);
                Chat chat = db.Chats.Include(x => x.Users).FirstOrDefault(c => c.Id_Chat == idChat);
                IEnumerable<User> users = chat.Users;
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получение пользователей голосового канала: {ex.Message}");
            }
            return null;
        }

        public List<User> GetServerAdminsByRole(int roleId)
        {
            try
            {
                using DataBaseContext db = new(_options);
                int serverId = db.Roles.FirstOrDefault(r => r.Id_Role == roleId).Server_Id;
                List<User> users = db.Servers.Include(s => s.Users)
                                             .ThenInclude(u => u.Roles)
                                             .FirstOrDefault(s => s.Id_Server == serverId)
                                     .Users.Where(u => u.Roles.Any(r => r.Name == "Admin"))
                                           .ToList();
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получение пользователей голосового канала: {ex.Message}");
            }

            return null;
        }
    }
}
