using System;
using System.IO;
using System.Xml;

namespace Messenger
{
    public class ConfigurationUserHelper
    {
        private string _path;
        private string _fileName;

        public ConfigurationUserHelper()
        {
            _path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Messenger";
            _fileName = "config";
            CheckExistsFile();
        }

        public ConfigurationUserHelper(string path, string fileName)
        {
            _path = path;
            _fileName = fileName;
            CheckExistsFile();
        }

        /// <summary>
        /// Запись пользователя в файл
        /// </summary>
        /// <param name="user">Пользователь для записи</param>
        public bool Write(User user)
        {
            CheckExistsFile();
            XmlDocument document = new();
            document.Load($"{_path}\\{_fileName}.xml");

            XmlNodeList childNodes = document.DocumentElement.SelectNodes("*");
            childNodes[0].InnerText = user.Email;
            childNodes[1].InnerText = user.Password;

            document.DocumentElement.SetAttribute("name", $"{user.Name} {user.SurName}");
            document.Save($"{_path}\\{_fileName}.xml");
            return true;
        }

        /// <summary>
        /// Чтение пользователя из файла
        /// </summary>
        /// <returns>Прочитанный пользователь</returns>
        public User Read()
        {
            if (CheckExistsFile())
            {
                XmlDocument document = new();
                document.Load($"{_path}\\{_fileName}.xml");

                XmlNodeList childNodes = document.DocumentElement.SelectNodes("*");

                return new User() { Email = childNodes[0].InnerText, Password = childNodes[1].InnerText };
            }
            else
                return new User();
        }

        /// <summary>
        /// Проверка на существование файла
        /// </summary>
        /// <returns>Существует ли файл</returns>
        private bool CheckExistsFile()
        {
            string pathFile = $"{_path}\\{_fileName}.xml";

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            if (!File.Exists(pathFile))
            {
                CreateConfigXml(pathFile);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Создание файла
        /// </summary>
        /// <param name="pathFile">Путь к файлу</param>
        private void CreateConfigXml(string pathFile)
        {
            XmlDocument xDoc = new();

            XmlElement userElem = xDoc.CreateElement("user");
            userElem.AppendChild(xDoc.CreateElement("email"));
            userElem.AppendChild(xDoc.CreateElement("password"));
            userElem.Attributes.Append(xDoc.CreateAttribute("name"));

            xDoc.AppendChild(userElem);
            xDoc.Save(pathFile);
        }
    }
}
