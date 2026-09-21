using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Friends.Responses;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends
{
    public record GetFriendsQuery(long UserId) : IQuery<IReadOnlyList<FriendshipResponse>>;

    public class GetFriendsQueryHandler(IDatabase database, IPresenceTracker presence)
        : IQueryHandler<GetFriendsQuery, IReadOnlyList<FriendshipResponse>>
    {
        public async ValueTask<IReadOnlyList<FriendshipResponse>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
        {
            var friendships = await database.Friendships.AsNoTracking()
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                    (f.RequesterId == request.UserId || f.AddresseeId == request.UserId))
                .ToListAsync(cancellationToken);

            return friendships.Select(f =>
            {
                var otherUserId = f.RequesterId == request.UserId ? f.AddresseeId : f.RequesterId;
                return FriendshipResponse.FromEntity(f, request.UserId, presence.GetStatus(otherUserId));
            }).ToList();
        }
    }
}
