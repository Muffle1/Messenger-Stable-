using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    [Table("Requests")]
    public class Request
    {
        [Key]
        public int Id_Request { get; set; }
        public string Sender { get; set; }
        [ForeignKey("Sender")]
        public User UserSender { get; set; }
        public string Receiver { get; set; }
        [ForeignKey("Receiver")]
        public User UserReceiver { get; set; }
    }
}
