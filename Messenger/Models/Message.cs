using System;

namespace Messenger
{
    public class Message : BaseModel, IFileHolder
    {
        public int Id_Message { get; set; }
        public string FromEmail { get; set; }
        public User From { get; set; }
        public string _fileName;
        public string FileName
        {
            get => _fileName;

            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }
        public byte[] File { get; set; }
        public DateTime? DateMessage { get; set; }
        public string Body { get; set; }
        public int Chat_Id { get; set; }
        public Chat Chat { get; set; }

        public Message(User user, Chat chat)
        {
            FromEmail = user.Email;
            Chat_Id = chat.Id_Chat;
        }

        public Message()
        {

        }
    }
}
