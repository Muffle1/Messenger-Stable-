using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace Messenger
{
    public static class FileHelper
    {
        private const long _maxFileSize = 10485760;

        public static IFileHolder OpenFile(IFileHolder fileHolder, string path)
        {
            try
            {
                long fileSize = new FileInfo(path).Length;

                if (fileSize < _maxFileSize)
                {
                    fileHolder.File = File.ReadAllBytes(path);
                    fileHolder.FileName = path.Split('\\')[^1];
                }

                return fileHolder;
            }
            catch
            {
                throw;
            }
        }

        public static bool SaveFile(string path, byte[] file)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                    File.WriteAllBytes(path, file);
                return File.Exists(path);
            }
            catch
            {
                throw;
            }
        }

        public static BitmapImage GetBitmapFromByteArray(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();

            return image;
        }
    }
}
