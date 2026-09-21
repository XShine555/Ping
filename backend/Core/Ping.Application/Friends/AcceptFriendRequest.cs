using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Friends.Responses;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends
{
    public record AcceptFriendRequestCommand(long CurrentUserId, Guid FriendshipId) : ICommand<ErrorOr<FriendshipResponse>>;

    public class AcceptFriendRequestCommandHandler(IDatabase database, IRealtimeNotifier notifier)
        : ICommandHandler<AcceptFriendRequestCommand, ErrorOr<FriendshipResponse>>
    {
        public async ValueTask<ErrorOr<FriendshipResponse>> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
        {
            var friendship = await database.Friendships
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .FirstOrDefaultAsync(f => f.Id == request.FriendshipId, cancellationToken);

            if (friendship is null)
                return Error.NotFound(description: $"Friend request {request.FriendshipId} not found");

            if (friendship.AddresseeId != request.CurrentUserId)
                return Error.Forbidden(description: "Only the addressee can accept this friend request");

            if (friendship.Status != FriendshipStatus.Pending)
                return Error.Conflict(description: "This friend request is no longer pending");

            friendship.Status = FriendshipStatus.Accepted;
            friendship.RespondedAt = DateTime.UtcNow;
            await database.SaveChangesAsync(cancellationToken);

            await notifier.FriendshipUpdatedAsync(friendship.RequesterId, friendship.Id, cancellationToken);

            return FriendshipResponse.FromEntity(friendship, request.CurrentUserId);
        }
    }
}
