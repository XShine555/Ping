using Ping.Application.Messages.Responses;

namespace Ping.Application.Contracts
{
    public interface IRealtimeNotifier
    {
        Task MessageCreatedAsync(Guid channelId, MessageResponse message, CancellationToken cancellationToken);

        Task MessageUpdatedAsync(Guid channelId, MessageResponse message, CancellationToken cancellationToken);

        Task MessageDeletedAsync(Guid channelId, Guid messageId, CancellationToken cancellationToken);

        Task FriendRequestReceivedAsync(long addresseeId, Guid friendshipId, CancellationToken cancellationToken);

        Task FriendshipUpdatedAsync(long userId, Guid friendshipId, CancellationToken cancellationToken);
    }
}
