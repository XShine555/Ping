using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ping.Domain.Abstractions;

namespace Ping.Domain.Entities
{
    [Table("User")]
    public class User : IAuditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required long Id { get; set; }

        [Required]
        [MaxLength(64)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(64)]
        public required string NormalizedUsername { get; set; }

        [MaxLength(64)]
        public string? DisplayName { get; set; }

        [MaxLength(2048)]
        public string? AvatarUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ServerMember> ServerMemberships { get; set; } = new List<ServerMember>();
    }
}
