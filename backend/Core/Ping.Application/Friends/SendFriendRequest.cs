using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Friends.Responses;
using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends
{
    public record SendFriendRequestCommand(long RequesterId, long AddresseeId) : ICommand<ErrorOr<FriendshipResponse>>;

    public class SendFriendRequestCommandHandler(IDatabase database, IRealtimeNotifier notifier)
        : ICommandHandler<SendFriendRequestCommand, ErrorOr<FriendshipResponse>>
    {
        public async ValueTask<ErrorOr<FriendshipResponse>> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.RequesterId == request.AddresseeId)
                return Error.Validation(description: "You cannot send a friend request to yourself");

            var addresseeExists = await database.Users.AnyAsync(u => u.Id == request.AddresseeId, cancellationToken);
            if (!addresseeExists)
                return Error.NotFound(description: $"User {request.AddresseeId} not found");

            var existing = await database.Friendships.FirstOrDefaultAsync(f =>
                (f.RequesterId == request.RequesterId && f.AddresseeId == request.AddresseeId) ||
                (f.RequesterId == request.AddresseeId && f.AddresseeId == request.RequesterId),
                cancellationToken);

            if (existing is not null)
            {
                return existing.Status switch
                {
                    FriendshipStatus.Blocked => Error.Forbidden(description: "This friendship is blocked"),
                    FriendshipStatus.Accepted => Error.Conflict(description: "You are already friends"),
                    _ => Error.Conflict(description: "A friend request already exists between these users"),
                };
            }

            var friendship = new Friendship
            {
                Id = Guid.NewGuid(),
                RequesterId = request.RequesterId,
                AddresseeId = request.AddresseeId,
            };
            await database.Friendships.AddAsync(friendship, cancellationToken);
            await database.SaveChangesAsync(cancellationToken);

            await notifier.FriendRequestReceivedAsync(request.AddresseeId, friendship.Id, cancellationToken);

            friendship.Requester = await database.Users.FirstAsync(u => u.Id == request.RequesterId, cancellationToken);
            friendship.Addressee = await database.Users.FirstAsync(u => u.Id == request.AddresseeId, cancellationToken);
            return FriendshipResponse.FromEntity(friendship, request.RequesterId);
        }
    }
}
