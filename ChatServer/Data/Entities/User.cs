using System.ComponentModel.DataAnnotations;

namespace ChatServer.Data.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        public Guid UID { get; set; }

        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }

        public string PasswordHash { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
