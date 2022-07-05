using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Server
{
    class Program
    {
        private static ServerContext _server;

        /// <summary>
        /// Запуск сервера
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        static void Main(string[] args)
        {
            try
            {
                CheckExistsDirectory();
                _server = new ServerContext();
                _server.Listen();
            }
            catch (Exception ex)
            {
                _server?.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Проверка создана ли папка для файлов
        /// </summary>
        /// <returns>Возращает если папка создана то true, иначе false</returns>
        private static void CheckExistsDirectory()
        {
            string pathDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Server\\Messages";
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);
            pathDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Server\\Servers";
            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);
        }
    }
}
