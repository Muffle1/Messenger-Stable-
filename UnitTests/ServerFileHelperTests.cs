using System;
using Xunit;
using Server;
using System.IO;
using System.Text;

namespace UnitTests
{
    public class ServerFileHelperTests
    {
        [Fact]
        public void SaveFileTest_SuccessfulSaving()
        {
            FileStream fileStream = File.Create("Test.txt");
            fileStream.Dispose();
            byte[] file = File.ReadAllBytes("Test.txt");
            Directory.CreateDirectory("TestingDirectory");

            var result = FileHelper.SaveFile("TestingDirectory", "Test.txt", file);
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

            Assert.Throws<DirectoryNotFoundException>(() => FileHelper.SaveFile("TestingDirectory", "Test.txt", file));

            File.Delete("Test.txt");
        }

        [Fact]
        public void LoadFileTest_SuccessfulLoadingNotEmptyFile()
        {
            Directory.CreateDirectory("TestingDirectory");
            FileStream fileStream = File.Create("TestingDirectory\\Test.txt");
            byte[] text = Encoding.UTF8.GetBytes("Test");
            fileStream.Write(text, 0, text.Length);
            fileStream.Dispose();
            byte[] fileExpected = File.ReadAllBytes("TestingDirectory\\Test.txt");

            byte[] fileActual = FileHelper.LoadFile("TestingDirectory", "Test.txt");
            Assert.Equal(fileExpected.Length, fileActual.Length);
            Assert.Equal(fileExpected, fileActual);
            Assert.NotEmpty(fileActual);

            Directory.Delete("TestingDirectory", true);
        }

        [Fact]
        public void LoadFileTest_SuccessfulLoadingEmptyFile()
        {
            Directory.CreateDirectory("TestingDirectory");
            FileStream fileStream = File.Create("TestingDirectory\\Test.txt");
            fileStream.Dispose();
            byte[] fileExpected = File.ReadAllBytes("TestingDirectory\\Test.txt");

            byte[] fileActual = FileHelper.LoadFile("TestingDirectory", "Test.txt");
            Assert.Equal(fileExpected.Length, fileActual.Length);
            Assert.Equal(fileExpected, fileActual);
            Assert.Empty(fileActual);

            Directory.Delete("TestingDirectory", true);
        }

        [Fact]
        public void LoadFileTest_FailLoadingNotExistingFile()
        {
            Directory.CreateDirectory("TestingDirectory");
            FileStream fileStream = File.Create("TestingDirectory\\Test.txt");
            byte[] text = Encoding.UTF8.GetBytes("Test");
            fileStream.Write(text, 0, text.Length);
            fileStream.Dispose();
            byte[] fileExpected = File.ReadAllBytes("TestingDirectory\\Test.txt");

            Assert.Throws<FileNotFoundException>(() => FileHelper.LoadFile("TestingDirectory", "Test1.txt"));

            Directory.Delete("TestingDirectory", true);
        }

        [Fact]
        public void LoadFileTest_FailLoadingNotExistingDirectory()
        {
            Directory.CreateDirectory("TestingDirectory");
            FileStream fileStream = File.Create("TestingDirectory\\Test.txt");
            byte[] text = Encoding.UTF8.GetBytes("Test");
            fileStream.Write(text, 0, text.Length);
            fileStream.Dispose();
            byte[] fileExpected = File.ReadAllBytes("TestingDirectory\\Test.txt");

            Assert.Throws<DirectoryNotFoundException>(() => FileHelper.LoadFile("Testing1Directory", "Test.txt"));

            Directory.Delete("TestingDirectory", true);
        }

        [Fact]
        public void DeleteFileTest_SuccessfulDeleting()
        {
            FileStream fileStream = File.Create("Test.txt");
            fileStream.Dispose();

            var result = FileHelper.DeleteFile("Test.txt");

            Assert.True(result);

            File.Delete("Test.txt");
        }

        [Fact]
        public void DeleteFileTest_DeletingNotExistingFile()
        {
            var result = FileHelper.DeleteFile("Test1.txt");

            Assert.False(result);
        }

        [Fact]
        public void DeleteFileByPattern_SuccessfulDeleting()
        {
            Directory.CreateDirectory("TestingDirectory");
            FileStream fileStream = File.Create("TestingDirectory\\Test.txt");
            fileStream.Dispose();

            var result = FileHelper.DeleteFile("TestingDirectory", "*.txt");

            Assert.True(result);

            Directory.Delete("TestingDirectory", true);
        }

        [Fact]
        public void DeleteFileByPattern_FailDeleting()
        {
            Directory.CreateDirectory("TestingDirectory");

            var result = FileHelper.DeleteFile("TestingDirectory", "*.txt");

            Assert.False(result);

            Directory.Delete("TestingDirectory", true);
        }
    }
}
