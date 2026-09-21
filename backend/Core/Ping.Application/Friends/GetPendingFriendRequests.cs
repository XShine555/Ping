using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Friends.Responses;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends
{
    public record GetPendingFriendRequestsQuery(long UserId) : IQuery<IReadOnlyList<FriendshipResponse>>;

    public class GetPendingFriendRequestsQueryHandler(IDatabase database)
        : IQueryHandler<GetPendingFriendRequestsQuery, IReadOnlyList<FriendshipResponse>>
    {
        public async ValueTask<IReadOnlyList<FriendshipResponse>> Handle(GetPendingFriendRequestsQuery request, CancellationToken cancellationToken)
        {
            var friendships = await database.Friendships.AsNoTracking()
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .Where(f => f.Status == FriendshipStatus.Pending &&
                    (f.RequesterId == request.UserId || f.AddresseeId == request.UserId))
                .ToListAsync(cancellationToken);

            return friendships.Select(f => FriendshipResponse.FromEntity(f, request.UserId)).ToList();
        }
    }
}
