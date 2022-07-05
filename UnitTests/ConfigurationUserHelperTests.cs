using Messenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace UnitTests
{
    public class ConfigurationUserHelperTests
    {
        [Fact]
        public void WriteTest_SuccessfulWriting()
        {
            ConfigurationUserHelper configurationUserHelper = new ConfigurationUserHelper(Environment.CurrentDirectory, "Test");
            User user = new()
            {
                Email = "Test@gmail.com",
                Password = "TestPass",
                Name = "Test",
                SurName = "Test"
            };

            var result = configurationUserHelper.Write(user);
            XmlDocument document = new();
            document.Load($"{Environment.CurrentDirectory}\\Test.xml");
            string name = document.DocumentElement.GetAttribute("name");
            XmlNodeList childNodes = document.DocumentElement.SelectNodes("*");
            User resultUser = new() 
            { 
                Email = childNodes[0].InnerText,
                Password = childNodes[1].InnerText 
            };
            Assert.True(result);
            Assert.Equal(name, $"{user.Name} {user.SurName}");
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(user.Password, resultUser.Password);

            File.Delete("Test.xml");
        }

        [Fact]
        public void ReadTest_SuccessfulReading()
        {
            ConfigurationUserHelper configurationUserHelper = new ConfigurationUserHelper(Environment.CurrentDirectory, "Test");
            User user = new()
            {
                Email = "Test@gmail.com",
                Password = "TestPass",
                Name = "Test",
                SurName = "Test"
            };

            XmlDocument document = new();
            document.Load($"{Environment.CurrentDirectory}\\Test.xml");

            XmlNodeList childNodes = document.DocumentElement.SelectNodes("*");
            childNodes[0].InnerText = user.Email;
            childNodes[1].InnerText = user.Password;
            document.DocumentElement.SetAttribute("name", $"{user.Name} {user.SurName}");
            document.Save($"{Environment.CurrentDirectory}\\Test.xml");
            var resultUser = configurationUserHelper.Read();
            Assert.NotNull(resultUser);
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(user.Password, resultUser.Password);

            File.Delete("Test.xml");
        }
    }
}