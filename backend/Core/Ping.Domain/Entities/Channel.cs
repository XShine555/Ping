using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ping.Domain.ValueObjects;

namespace Ping.Domain.Entities
{
    [Table("Channel")]
    public class Channel
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? ServerId { get; set; }

        public Server? ServerNavigation { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(1024)]
        public string? Topic { get; set; }

        [Required]
        public ChannelType Type { get; set; }

        [Required]
        public int Position { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChannelMember> Members { get; set; } = new List<ChannelMember>();
    }
}
