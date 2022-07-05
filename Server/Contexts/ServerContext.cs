using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public class ServerContext
    {
        public readonly DataBaseHelper DataBaseHelper;

        private static TcpListener _tcpListener;
        private readonly List<ClientContext> _clients = new();
        private BinaryWriter _writer;

        public ServerContext()
        {
            DataBaseHelper = new(GetContextOptions());
        }

        private static DbContextOptions GetContextOptions()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("databasesettings.json");
            var config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultConnection");
            DbContextOptionsBuilder optionsBuilder = new();
            optionsBuilder.UseSqlServer(connectionString);
            return optionsBuilder.Options;
        }

        /// <summary>
        /// Добавление подключения к серверу
        /// </summary>
        /// <param name="clientModel">Клиент</param>
        public void AddConnection(ClientContext clientModel) =>
            _clients.Add(clientModel);

        /// <summary>
        /// Удаление клиента
        /// </summary>
        /// <param name="email">Почта клиента</param>
        public void RemoveConnection(string email)
        {
            ClientContext client = _clients.FirstOrDefault(c => c.UserEmail == email);

            if (client != null)
                _clients.Remove(client);
        }

        /// <summary>
        /// Чтение потока подключений
        /// </summary>
        public void Listen()
        {
            //SendData();
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, 8005);
                _tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                    ClientContext clientModel = new(tcpClient, this);
                    Thread clientThread = new(new ThreadStart(clientModel.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        /// <summary>
        /// Проверка на подключение пользователя к серверу
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Подключенный пользователь</returns>
        protected internal bool IsConnected(string email) =>
            _clients.Any(c => c.UserEmail == email);

        /// <summary>
        /// Отправка ответа пользователю
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="command">Команда</param>
        /// <param name="message">Сообщения</param>
        /// <param name="userEmail">Пользователь</param>
        protected internal void SendResponse(Package package, string userEmail)
        {
            ClientContext clientModel = _clients.FirstOrDefault(c => c.UserEmail == userEmail);
            if (clientModel != null)
            {
                _writer = new BinaryWriter(clientModel.Stream);
                _writer.Write(JsonConvert.SerializeObject(package, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                _writer.Flush();
            }
        }

        /// <summary>
        /// Рассылка сообщений пользователям в чате
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="chatId">Чат</param>
        protected internal void BroadcastMessage(Message message, int chatId)
        {
            List<User> users = DataBaseHelper.GetChatUsers(chatId);
            ClientContext[] clientModels = _clients.Where(c => users.Any(x => x.Email == c.UserEmail)).ToArray();
            for (int i = 0; i < clientModels.Length; i++)
            {
                Package package = new(Command.Message, message);
                SendResponse(package, clientModels[i].UserEmail);
            }
        }

        /// <summary>
        /// Отключение сервера
        /// </summary>
        protected internal void Disconnect()
        {
            _tcpListener.Stop();

            for (int i = 0; i < _clients.Count; i++)
                _clients[i].Close();

            if (_writer != null)
                _writer.Close();
            Environment.Exit(0);
        }
    }
}
