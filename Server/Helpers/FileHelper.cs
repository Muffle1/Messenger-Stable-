using System;
using System.IO;

namespace Server
{
    public static class FileHelper
    {
        public static bool SaveFile(string path, string fileName, byte[] file)
        {
            try
            {
                path = $"{path}\\{fileName}";
                File.WriteAllBytes(path, file);
                return File.Exists(path);
            }
            catch
            {
                throw;
            }
        }

        public static byte[] LoadFile(string path, string fileName)
        {
            try
            {
                path = $"{path}\\{fileName}";
                byte[] file = File.ReadAllBytes(path);
                return file;
            }
            catch
            {
                throw;
            }
        }

        public static bool DeleteFile(string directoryPath, string pattern)
        {
            try
            {
                var dir = new DirectoryInfo(directoryPath);
                foreach (var file in dir.EnumerateFiles(pattern))
                {
                    file.Delete();
                    return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        public static bool DeleteFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return false;

                File.Delete(path);
                return File.Exists(path) ? false : true;
            }
            catch
            {
                throw;
            }
        }
    }
}
