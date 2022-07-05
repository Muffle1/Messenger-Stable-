using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    public class DataBaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserChat> UserChats { get; set; }

        public DataBaseContext(DbContextOptions options) : base(options)
        {

        }

        /// <summary>
        /// Создание моделей
        /// </summary>
        /// <param name="modelBuilder">Модельный строитель</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Многие ко многим чаты с пользователями
            modelBuilder.Entity<Chat>()
                        .HasMany(c => c.Users)
                        .WithMany(u => u.Chats)
                        .UsingEntity<UserChat>(
                            j => j.HasOne(cu => cu.User)
                                  .WithMany(u => u.UserChats)
                                  .HasForeignKey(cu => cu.Email),
                            j => j.HasOne(cu => cu.Chat)
                                  .WithMany(c => c.UserChats)
                                  .HasForeignKey(cu => cu.Chat_Id),
                            j => j.ToTable("UsersInChats")
                                  .HasKey(j => new { j.Email, j.Chat_Id }));

            // Многие ко многим роли с пользователи
            modelBuilder.Entity<Role>()
                        .HasMany(u => u.Users)
                        .WithMany(r => r.Roles)
                        .UsingEntity<Dictionary<string, object>>(
                                "RolesInUsers",
                                j => j.HasOne<User>()
                                    .WithMany()
                                    .HasForeignKey("User_Email"),
                                j => j
                                    .HasOne<Role>()
                                    .WithMany()
                                    .HasForeignKey("Role_Id"));

            // Многие ко многим чаты с роли
            modelBuilder.Entity<Role>()
                        .HasMany(c => c.Chats)
                        .WithMany(u => u.Roles)
                        .UsingEntity<Dictionary<string, object>>(
                                "ChatsInRoles",
                                j => j.HasOne<Chat>()
                                    .WithMany()
                                    .HasForeignKey("Chat_Id"),
                                j => j
                                    .HasOne<Role>()
                                    .WithMany()
                                    .HasForeignKey("Role_Id"));

            // Многие ко многим пользователи и сервера
            modelBuilder.Entity<User>()
                        .HasMany(u => u.Servers)
                        .WithMany(r => r.Users)
                        .UsingEntity<Dictionary<string, object>>(
                                "UsersOnServers",
                                j => j.HasOne<Server>()
                                    .WithMany()
                                    .HasForeignKey("Server_Id"),
                                j => j
                                    .HasOne<User>()
                                    .WithMany()
                                    .HasForeignKey("User_Email"));

            //Мб сработает  чтобы друзья подтягивались через Include (надо протестить)
            //modelBuilder.Entity<User>()
            //            .HasMany(x => x.Friends)
            //            .WithMany(x => x.FriendsTest)
            //            .UsingEntity<Friendship>(
            //            x => x.HasOne(cu => cu.Friend1).WithMany(cu => cu.Friendships).HasForeignKey(cu => cu.Friend1Email),
            //            x => x.HasOne(cu => cu.Friend2).WithMany(cu => cu.FriendshipsTest).HasForeignKey(cu => cu.Friend2Email),
            //            x => x.ToTable("Friendships").HasKey(x => x.Id_Friendship));

            modelBuilder.Entity<Request>().HasOne(r => r.UserReceiver).WithMany(u => u.EnterRequests).HasForeignKey(u => u.Receiver);
            modelBuilder.Entity<Request>().HasOne(r => r.UserSender).WithMany(u => u.SendRequests).HasForeignKey(u => u.Sender);

            modelBuilder.Entity<Friendship>().HasKey(fs => new { fs.Friend1Email, fs.Friend2Email });
        }
    }
}