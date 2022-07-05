using System.ComponentModel.DataAnnotations;

namespace Server
{
    public class Settings
    {
        [Key]
        public int Id_Settings { get; set; }
        public bool Notice { get; set; }

        public Settings(bool notice)
        {
            Notice = notice;
        }

        public Settings()
        {

        }
    }
}
