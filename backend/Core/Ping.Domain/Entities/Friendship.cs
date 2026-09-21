using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ping.Domain.ValueObjects;

namespace Ping.Domain.Entities
{
    [Table("Friendship")]
    public class Friendship
    {
        [Key]
        public Guid Id { get; set; }

        public required long RequesterId { get; set; }

        public User? Requester { get; set; }

        public required long AddresseeId { get; set; }

        public User? Addressee { get; set; }

        [Required]
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public long? BlockedById { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }
    }
}
