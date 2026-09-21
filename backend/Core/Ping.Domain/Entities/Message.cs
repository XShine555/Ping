using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ping.Domain.Entities
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid ChannelId { get; set; }

        public Channel? Channel { get; set; }

        public required long AuthorId { get; set; }

        public User? Author { get; set; }

        [Required]
        [MaxLength(4000)]
        public required string Content { get; set; }

        public Guid? ReplyToMessageId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EditedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
