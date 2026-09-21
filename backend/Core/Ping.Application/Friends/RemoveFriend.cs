using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends
{
    public record RemoveFriendCommand(long CurrentUserId, Guid FriendshipId) : ICommand<ErrorOr<Success>>;

    public class RemoveFriendCommandHandler(IDatabase database, IRealtimeNotifier notifier)
        : ICommandHandler<RemoveFriendCommand, ErrorOr<Success>>
    {
        public async ValueTask<ErrorOr<Success>> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
        {
            var friendship = await database.Friendships.FirstOrDefaultAsync(f => f.Id == request.FriendshipId, cancellationToken);

            if (friendship is null)
                return Error.NotFound(description: $"Friendship {request.FriendshipId} not found");

            if (friendship.RequesterId != request.CurrentUserId && friendship.AddresseeId != request.CurrentUserId)
                return Error.Forbidden(description: "You are not part of this friendship");

            if (friendship.Status != FriendshipStatus.Accepted)
                return Error.Conflict(description: "This friendship is not active");

            var otherUserId = friendship.RequesterId == request.CurrentUserId ? friendship.AddresseeId : friendship.RequesterId;

            database.Friendships.Remove(friendship);
            await database.SaveChangesAsync(cancellationToken);

            await notifier.FriendshipUpdatedAsync(otherUserId, friendship.Id, cancellationToken);

            return new Success();
        }
    }
}
