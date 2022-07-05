namespace Server
{
    public class Attachment
    {
        public string FileName { get; set; }
        public byte[] File { get; set; }

        public Attachment(string fileName, byte[] file)
        {
            FileName = fileName;
            File = file;
        }
    }
}
