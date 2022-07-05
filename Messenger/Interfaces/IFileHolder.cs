namespace Messenger
{
    public interface IFileHolder
    {
        public string FileName { get; set; }
        public byte[] File { get; set; }
    }
}
