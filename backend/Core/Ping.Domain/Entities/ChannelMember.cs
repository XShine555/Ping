namespace Ping.Domain.Entities
{
    public class ChannelMember
    {
        public required Guid ChannelId { get; set; }

        public Channel? Channel { get; set; }

        public required long UserId { get; set; }

        public User? User { get; set; }
    }
}
