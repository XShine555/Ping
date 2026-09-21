using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends
{
    public record RejectFriendRequestCommand(long CurrentUserId, Guid FriendshipId) : ICommand<ErrorOr<Success>>;

    public class RejectFriendRequestCommandHandler(IDatabase database, IRealtimeNotifier notifier)
        : ICommandHandler<RejectFriendRequestCommand, ErrorOr<Success>>
    {
        public async ValueTask<ErrorOr<Success>> Handle(RejectFriendRequestCommand request, CancellationToken cancellationToken)
        {
            var friendship = await database.Friendships.FirstOrDefaultAsync(f => f.Id == request.FriendshipId, cancellationToken);

            if (friendship is null)
                return Error.NotFound(description: $"Friend request {request.FriendshipId} not found");

            if (friendship.AddresseeId != request.CurrentUserId)
                return Error.Forbidden(description: "Only the addressee can reject this friend request");

            if (friendship.Status != FriendshipStatus.Pending)
                return Error.Conflict(description: "This friend request is no longer pending");

            database.Friendships.Remove(friendship);
            await database.SaveChangesAsync(cancellationToken);

            await notifier.FriendshipUpdatedAsync(friendship.RequesterId, friendship.Id, cancellationToken);

            return new Success();
        }
    }
}
