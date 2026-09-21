using Ping.Domain.ValueObjects;

namespace Ping.Api.DataTransferObjects.Channels;

public record CreateChannelRequest(string Name, ChannelType Type);
