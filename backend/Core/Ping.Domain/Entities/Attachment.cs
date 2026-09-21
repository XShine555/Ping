using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ping.Domain.ValueObjects;

namespace Ping.Domain.Entities
{
    [Table("Attachment")]
    public class Attachment
    {
        [Key]
        public Guid Id { get; set; }

        public required Guid MessageId { get; set; }

        public Message? Message { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Bucket { get; set; }

        [Required]
        [MaxLength(1024)]
        public required string Key { get; set; }

        [Required]
        [MaxLength(255)]
        public required string FileName { get; set; }

        [Required]
        [MaxLength(255)]
        public required string ContentType { get; set; }

        public required long SizeBytes { get; set; }

        [Required]
        public AttachmentKind Kind { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
