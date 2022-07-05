using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Server;
using Microsoft.EntityFrameworkCore;

namespace UnitTests
{
    public class DataBaseHelperTests
    {
        private DbContextOptions _option;
        private DataBaseHelper dataBaseHelper;

        private const string email = "TestEmail@gmail.com";
        private const string login = "TestLogin";

        public DataBaseHelperTests()
        {
            DbContextOptionsBuilder optionsBuilder = new();
            optionsBuilder.UseInMemoryDatabase("TestDataBase");
            _option = optionsBuilder.Options;

            dataBaseHelper = new DataBaseHelper(_option);
        }


        private User CreateTestingUser(string email, string login)
        {
            User user = new()
            {
                Email = email,
                Name = "Testname",
                SurName = "Testsurname",
                PhoneNumber = "7(900)010-00-00",
                Login = login,
                Password = "TestPass",
                Birthday = new DateTime(2000, 12, 1),
                Settings = new()
                {
                    Notice = true
                }
            };
            return user;
        }

        private void RemoveChat(int id)
        {
            using (var db = new DataBaseContext(_option))
            {
                Chat chat = db.Chats.FirstOrDefault(r => r.Id_Chat == id);
                db.Chats.Remove(chat);
                db.SaveChanges();
            }
        }

        private void RemoveRequest(int id)
        {
            using (var db = new DataBaseContext(_option))
            {
                Request request = db.Requests.FirstOrDefault(r => r.Id_Request == id);
                db.Requests.Remove(request);
                db.SaveChanges();
            }
        }

        private void RemoveUser(string email)
        {
            using (var db = new DataBaseContext(_option))
            {
                db.Users.Remove(db.Users.First(u => u.Email == email));
                db.SaveChanges();
            }

        }

        private void RemoveFriendship(Friendship friendship)
        {
            using DataBaseContext db = new(_option);
            db.Friendships.Remove(friendship);
            db.SaveChanges();
        }

        private void AddUser(User user)
        {
            using (var db = new DataBaseContext(_option))
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
        }

        private void AddFriendship(Friendship friendship)
        {
            using (var db = new DataBaseContext(_option))
            {
                db.Friendships.Add(friendship);
                db.SaveChanges();
            }
        }

        private void AddChat(Chat chat)
        {
            using (var db = new DataBaseContext(_option))
            {
                db.Chats.Add(chat);
                db.SaveChanges();
            }
        }

        private User GetUser(string email)
        {
            User user;
            using (var db = new DataBaseContext(_option))
            {
                user = db.Users.FirstOrDefault(u => u.Email == email);
            }
            return user;
        }

        [Fact]
        public void AddUser_SuccessfulAdding()
        {
            User user = CreateTestingUser(email, login);

            User resultUser = dataBaseHelper.AddUser(user);

            Assert.NotNull(resultUser);
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(user.Name, resultUser.Name);
            Assert.Equal(user.SurName, resultUser.SurName);
            Assert.Equal(user.PhoneNumber, resultUser.PhoneNumber);
            Assert.Equal(user.Login, resultUser.Login);
            Assert.Equal(user.Password, resultUser.Password);
            Assert.Equal(user.Birthday, resultUser.Birthday);

            RemoveUser(email);
        }

        [Fact]
        public void AddUser_FailAddingSameUser()
        {
            User user = CreateTestingUser(email, login);

            dataBaseHelper.AddUser(user);
            User resultUser = dataBaseHelper.AddUser(user);

            Assert.Null(resultUser);

            RemoveUser(email);
        }

        [Fact]
        public void GetUserByEmail_SuccessfulGetting()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);

            User resultUser = dataBaseHelper.GetUser(user.Email);

            Assert.NotNull(resultUser);
            Assert.Equal(user.Email, resultUser.Email);

            RemoveUser(email);
        }

        [Fact]
        public void GetUserByEmail_FailGetting()
        {
            User resultUser = dataBaseHelper.GetUser("TestEmail");

            Assert.Null(resultUser);
        }

        [Fact]
        public void GetUserByEmailAndPass_SuccessfulGetting()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);

            User resultUser = dataBaseHelper.GetUser(user.Email, user.Password);

            Assert.NotNull(resultUser);
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(user.Password, resultUser.Password);
            Assert.NotNull(resultUser.Settings);
            Assert.NotNull(resultUser.Servers);

            RemoveUser(email);
        }

        [Fact]
        public void GetUserByEmailAndPass_FailGetting()
        {
            User resultUser = dataBaseHelper.GetUser(email, "TestPass");

            Assert.Null(resultUser);
        }

        [Fact]
        public void GetSearchUsers_SuccessfulGettingByNotFullEmail()
        {
            User user1 = CreateTestingUser("Test1Email@gmail.com", "Test1Login");
            User user2 = CreateTestingUser("Test2Email@gmail.com", "Test2Login");
            AddUser(user1);
            AddUser(user2);

            User[] result = dataBaseHelper.GetSearchUsers("Test2Email", user1.Email);
            Assert.NotNull(result);
            Assert.Contains("Test2Email", result[0].Email);
            Assert.NotNull(result[0].Login);

            RemoveUser(user1.Email);
            RemoveUser(user2.Email);
        }

        [Fact]
        public void GetSearchUsers_SuccessfulGettingByFullEmail()
        {
            User user1 = CreateTestingUser("Test1Email@gmail.com", "Test1Login");
            User user2 = CreateTestingUser("Test2Email@gmail.com", "Test2Login");
            AddUser(user1);
            AddUser(user2);

            User[] result = dataBaseHelper.GetSearchUsers("Test2Email@gmail.com", user1.Email);
            Assert.NotNull(result);
            Assert.Equal("Test2Email@gmail.com", result[0].Email);
            Assert.NotNull(result[0].Login);

            RemoveUser(user1.Email);
            RemoveUser(user2.Email);
        }

        [Fact]
        public void GetSearchUsers_SuccessfulGettingByLogin()
        {
            User user1 = CreateTestingUser("Test1Email@gmail.com", "Test1Login");
            User user2 = CreateTestingUser("Test2Email@gmail.com", "Test2Login");
            AddUser(user1);
            AddUser(user2);

            User[] result = dataBaseHelper.GetSearchUsers("Test2Login", user1.Email);
            Assert.NotNull(result);
            Assert.Equal("Test2Login", result[0].Login);
            Assert.NotNull(result[0].Email);

            RemoveUser(user1.Email);
            RemoveUser(user2.Email);
        }

        [Fact]
        public void GetSearchUsers_FailGetting()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);

            User[] result = dataBaseHelper.GetSearchUsers(email, user.Email);

            Assert.Empty(result);

            RemoveUser(user.Email);
        }

        [Fact]
        public void SaveChangeUser_SuccessfulSaving()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);

            user.Name = "Name";
            var result = dataBaseHelper.SaveChangeUser(user);
            User resultUser = GetUser(user.Email);

            Assert.True(result);
            Assert.NotNull(resultUser);
            Assert.Equal(user.Name, resultUser.Name);

            RemoveUser(user.Email);
        }

        [Fact]
        public void AddRequest_SuccessfulAdding()
        {
            User user1 = CreateTestingUser(email, login);
            User user2 = CreateTestingUser("Test2Email@gmail.com", "Test2Login");
            AddUser(user1);
            AddUser(user2);

            Request request = new()
            {
                Sender = user1.Email,
                UserSender = new() { Email = user1.Email },
                Receiver = user2.Email,
                UserReceiver = new() { Email = user2.Email }
            };

            Request result = dataBaseHelper.AddRequest(request);

            Assert.NotNull(result);
            Assert.Equal(request.Sender, result.Sender);
            Assert.Equal(request.Receiver, result.Receiver);

            RemoveUser(user1.Email);
            RemoveUser(user2.Email);
            if (result != null)
                RemoveRequest(result.Id_Request);
        }

        [Fact]
        public void AddRequest_FailAdding()
        {
            User user1 = CreateTestingUser("Test1Email@gmail.com", "Test1Login");
            User user2 = CreateTestingUser("Test2Email@gmail.com", "Test2Login");
            AddUser(user1);
            AddUser(user2);

            Request request = new()
            {
                Sender = user1.Email,
                UserSender = user1,
                Receiver = user2.Email,
                UserReceiver = user2
            };

            dataBaseHelper.AddRequest(request);
            var result = dataBaseHelper.AddRequest(request);

            Assert.Null(result);

            RemoveUser(user1.Email);
            RemoveUser(user2.Email);
            if(result != null)
                RemoveRequest(result.Id_Request);
        }

        [Fact]
        public void AddChat_SuccessfulAdding()
        {
            User user = CreateTestingUser("Test1Email@gmail.com", "Test1Login");
            AddUser(user);

            Chat chat = new()
            {
                Name = "TestChatName",
                IsVoiceChat = false,
            };
            chat.UserChats.Add(new UserChat() { Email = user.Email, IsAdmin = true });

            Chat resultChat = dataBaseHelper.AddChat(chat);

            Assert.NotNull(resultChat);
            Assert.Equal(chat.Name, resultChat.Name);
            Assert.Equal(chat.UserChats[0].Email, resultChat.UserChats[0].Email);

            RemoveUser(user.Email);
            if (resultChat != null)
                RemoveChat(resultChat.Id_Chat);
        }

        [Fact]
        public void GetChat_SuccessfulGetting()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);

            Chat chat = new()
            {
                Name = "TestChatName",
                IsVoiceChat = false,
            };
            chat.UserChats.Add(new UserChat() { Email = user.Email, IsAdmin = true });
            AddChat(chat);

            Chat resultChat = dataBaseHelper.GetChat(chat.Id_Chat);

            Assert.NotNull(resultChat);
            Assert.Equal(chat.Id_Chat, resultChat.Id_Chat);

            RemoveChat(chat.Id_Chat);
            RemoveUser(user.Email);
        }

        [Fact]
        public void GetChat_FailGetting()
        {
            Chat resultChat = dataBaseHelper.GetChat(1);

            Assert.Null(resultChat);
        }

        [Fact]
        public void RemoveChat_SuccessfulRemoving()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);

            Chat chat = new()
            {
                Name = "TestChatName",
                IsVoiceChat = false,
            };
            chat.UserChats.Add(new UserChat() { Email = user.Email, IsAdmin = true });

            AddChat(chat);

            var result = dataBaseHelper.RemoveChat(chat.Id_Chat);

            Assert.True(result);

            RemoveUser(user.Email);
            if (!result)
                RemoveChat(chat.Id_Chat);
        }

        [Fact]
        public void GetChats_SuccessfulGetting()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);
            Chat chat = new()
            {
                Name = "TestChatName",
                IsVoiceChat = false,
            };
            chat.UserChats.Add(new UserChat() { Email = user.Email, IsAdmin = true });
            AddChat(chat);

            List<Chat> resultChat = dataBaseHelper.GetChats(user.Email);

            Assert.NotEmpty(resultChat);
            Assert.Equal(chat.Id_Chat, resultChat[0].Id_Chat);

            RemoveUser(user.Email);
            RemoveChat(chat.Id_Chat);
        }

        [Fact]
        public void GetChats_GettingChatsUserWithoutChats()
        {
            User user = CreateTestingUser(email, login);
            AddUser(user);

            List<Chat> resultChat = dataBaseHelper.GetChats(user.Email);

            Assert.Empty(resultChat);

            RemoveUser(user.Email);
        }

        [Fact]
        public void GetFriends_SuccessfulGetting()
        {
            User user1 = CreateTestingUser(email, login);
            User user2 = CreateTestingUser("Test2Email@gmail.com", "Test2Login");

            AddUser(user1);
            AddUser(user2);

            Friendship friendship = new(user1.Email, user2.Email);
            AddFriendship(friendship);

            List<User> users = dataBaseHelper.GetFriends(user1.Email);
            Assert.NotEmpty(users);
            Assert.Equal(users[0].Email, user2.Email);

            RemoveUser(user1.Email);
            RemoveUser(user2.Email);
            RemoveFriendship(friendship);
        }


        [Fact]
        public void GetFriends_GettingFriendsNonExistsUser()
        {
            List<User> users = dataBaseHelper.GetFriends("Test@gmail.com");
            Assert.Empty(users);
        }


    }
}
