using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Messenger;
using System.IO;

namespace UnitTests
{
    public class ClientFileHelperTests
    {
        [Fact]
        public void OpenFileTest_SuccessfulOpening()
        {
            Messenger.Server server = new Messenger.Server();
            FileStream fileStream = File.Create("Test.txt");
            byte[] text = Encoding.UTF8.GetBytes("Test");
            fileStream.Write(text, 0, text.Length);
            fileStream.Dispose();
            byte[] fileExpected = File.ReadAllBytes("Test.txt");

            server = FileHelper.OpenFile(server, "Test.txt") as Messenger.Server;
            Assert.Equal(fileExpected.Length, server.File.Length);
            Assert.Equal(fileExpected, server.File);
            Assert.Equal("Test.txt", server.FileName);
            Assert.NotEmpty(server.FileName);
            Assert.NotEmpty(server.File);

            File.Delete("Test.txt");
        }

        [Fact]
        public void OpenFileTest_FailOpening()
        {
            Messenger.Server server = new Messenger.Server();

            Assert.Throws<FileNotFoundException>(() => FileHelper.OpenFile(server, "Test1.txt"));

            File.Delete("Test.txt");
        }

        [Fact]
        public void OpenFileTest_FailOpeningBigFile()
        {
            Messenger.Server server = new Messenger.Server();
            FileStream fileStream = File.Create("Test.txt");
            for (int i = 0; i <= 10485761; ++i)
                fileStream.Write(Encoding.UTF8.GetBytes("1"), 0, 1);
            fileStream.Dispose();

            server = FileHelper.OpenFile(server, "Test.txt") as Messenger.Server;
            Assert.Null(server.File);

            File.Delete("Test.txt");
        }

        [Fact]
        public void SaveFileTest_SuccessfulSaving()
        {
            FileStream fileStream = File.Create("Test.txt");
            fileStream.Dispose();
            byte[] file = File.ReadAllBytes("Test.txt");
            Directory.CreateDirectory("TestingDirectory");

            var result = FileHelper.SaveFile("TestingDirectory\\Test.txt", file);
            Assert.True(result);

            File.Delete("Test.txt");
            Directory.Delete("TestingDirectory", true);
        }

        [Fact]
        public void SaveFileTest_FailSaving()
        {
            FileStream fileStream = File.Create("Test.txt");
            fileStream.Dispose();
            byte[] file = File.ReadAllBytes("Test.txt");

            Assert.Throws<DirectoryNotFoundException>(() => FileHelper.SaveFile("Testing1Directory\\Test.txt", file));

            File.Delete("Test.txt");
        }

        [Fact]
        public void GetBitmapFromByteArrayTest_SuccessfulGetting()
        {
            byte[] file = File.ReadAllBytes("TestingObjects\\TestingObject1.jpg");

            var result = FileHelper.GetBitmapFromByteArray(file);

            Assert.NotNull(result);
        }

        [Fact]
        public void GetBitmapFromByteArrayTest_FailGetting()
        {
            var result = FileHelper.GetBitmapFromByteArray(null);

            Assert.Null(result);
        }
    }
}
