using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Channels.Responses
{
    public record ChannelResponse(Guid Id, Guid? ServerId, string? Name, ChannelType Type, int Position)
    {
        public static ChannelResponse FromEntity(Channel channel) =>
            new(channel.Id, channel.ServerId, channel.Name, channel.Type, channel.Position);
    }
}
