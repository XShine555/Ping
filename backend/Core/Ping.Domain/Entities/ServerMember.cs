using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ping.Domain.ValueObjects;

namespace Ping.Domain.Entities
{
    [Table("ServerMember")]
    public class ServerMember
    {
        public required Guid ServerId { get; set; }

        public Server? Server { get; set; }

        public required long UserId { get; set; }

        public User? User { get; set; }

        [MaxLength(64)]
        public string? Nickname { get; set; }

        [Required]
        public ServerMemberRole Role { get; set; } = ServerMemberRole.Member;

        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
