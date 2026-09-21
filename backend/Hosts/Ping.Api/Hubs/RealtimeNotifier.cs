using Microsoft.AspNetCore.SignalR;
using Ping.Application.Contracts;
using Ping.Application.Messages.Responses;

namespace Ping.Api.Hubs;

public sealed class RealtimeNotifier(IHubContext<ChatHub> hubContext) : IRealtimeNotifier
{
    public Task MessageCreatedAsync(Guid channelId, MessageResponse message, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(ChatHub.GroupName(channelId)).SendAsync("MessageCreated", message, cancellationToken);

    public Task MessageUpdatedAsync(Guid channelId, MessageResponse message, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(ChatHub.GroupName(channelId)).SendAsync("MessageUpdated", message, cancellationToken);

    public Task MessageDeletedAsync(Guid channelId, Guid messageId, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(ChatHub.GroupName(channelId)).SendAsync("MessageDeleted", messageId, cancellationToken);

    public Task FriendRequestReceivedAsync(long addresseeId, Guid friendshipId, CancellationToken cancellationToken) =>
        hubContext.Clients.User(addresseeId.ToString()).SendAsync("FriendRequestReceived", friendshipId, cancellationToken);

    public Task FriendshipUpdatedAsync(long userId, Guid friendshipId, CancellationToken cancellationToken) =>
        hubContext.Clients.User(userId.ToString()).SendAsync("FriendshipUpdated", friendshipId, cancellationToken);
}
