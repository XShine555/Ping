using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Friends
{
    public record BlockUserCommand(long CurrentUserId, long TargetUserId) : ICommand<ErrorOr<Success>>;

    public class BlockUserCommandHandler(IDatabase database, IRealtimeNotifier notifier)
        : ICommandHandler<BlockUserCommand, ErrorOr<Success>>
    {
        public async ValueTask<ErrorOr<Success>> Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentUserId == request.TargetUserId)
                return Error.Validation(description: "You cannot block yourself");

            var friendship = await database.Friendships.FirstOrDefaultAsync(f =>
                (f.RequesterId == request.CurrentUserId && f.AddresseeId == request.TargetUserId) ||
                (f.RequesterId == request.TargetUserId && f.AddresseeId == request.CurrentUserId),
                cancellationToken);

            if (friendship is null)
            {
                friendship = new Friendship
                {
                    Id = Guid.NewGuid(),
                    RequesterId = request.CurrentUserId,
                    AddresseeId = request.TargetUserId,
                };
                await database.Friendships.AddAsync(friendship, cancellationToken);
            }

            friendship.Status = FriendshipStatus.Blocked;
            friendship.BlockedById = request.CurrentUserId;
            friendship.RespondedAt = DateTime.UtcNow;
            await database.SaveChangesAsync(cancellationToken);

            await notifier.FriendshipUpdatedAsync(request.TargetUserId, friendship.Id, cancellationToken);

            return new Success();
        }
    }
}
