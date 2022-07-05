using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        public int Id_Message { get; set; }
        [Column("From")]
        public string FromEmail { get; set; }
        [ForeignKey("FromEmail")]
        public User From { get; set; }
        public string FileName { get; set; }
        [NotMapped]
        public byte[] File { get; set; }
        public DateTime? DateMessage { get; set; }
        public string Body { get; set; }
        public int Chat_Id { get; set; }
        [ForeignKey("Chat_Id")]
        public Chat Chat { get; set; }
    }
}
