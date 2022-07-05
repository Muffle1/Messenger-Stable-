namespace Messenger
{
    public class Attachment : BaseModel
    {
        public string FileName { get; set; }
        public byte[] File { get; set; }
    }
}
