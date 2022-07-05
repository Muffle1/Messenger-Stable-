using System.ComponentModel.DataAnnotations.Schema;

namespace Server
{
    public class Friendship
    {
        [Column("Friend1")]
        public string Friend1Email { get; set; }
        [ForeignKey("Friend1Email")]
        public User Friend1 { get; set; }
        [Column("Friend2")]
        public string Friend2Email { get; set; }
        [ForeignKey("Friend2Email")]
        public User Friend2 { get; set; }

        public Friendship(string friend1, string friend2)
        {
            Friend1Email = friend1;
            Friend2Email = friend2;
        }

        public Friendship()
        {

        }
    }
}
