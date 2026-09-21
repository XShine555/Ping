using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ping.Domain.Abstractions;

namespace Ping.Domain.Entities
{
    [Table("Server")]
    public class Server : IAuditable
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [MaxLength(2048)]
        public string? IconUrl { get; set; }

        public required long OwnerId { get; set; }

        public User? Owner { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServerMember> Members { get; set; } = new List<ServerMember>();

        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    }
}
