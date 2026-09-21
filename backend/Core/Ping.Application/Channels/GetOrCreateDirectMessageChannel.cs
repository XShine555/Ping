using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Channels.Responses;
using Ping.Application.Contracts;
using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Channels
{
    public record GetOrCreateDirectMessageChannelCommand(long CurrentUserId, long OtherUserId)
        : ICommand<ErrorOr<ChannelResponse>>;

    public class GetOrCreateDirectMessageChannelCommandHandler(IDatabase database)
        : ICommandHandler<GetOrCreateDirectMessageChannelCommand, ErrorOr<ChannelResponse>>
    {
        public async ValueTask<ErrorOr<ChannelResponse>> Handle(
            GetOrCreateDirectMessageChannelCommand request, CancellationToken cancellationToken)
        {
            if (request.CurrentUserId == request.OtherUserId)
                return Error.Validation(description: "You cannot open a direct message channel with yourself");

            var areFriends = await database.Friendships.AnyAsync(f =>
                f.Status == FriendshipStatus.Accepted &&
                ((f.RequesterId == request.CurrentUserId && f.AddresseeId == request.OtherUserId) ||
                 (f.RequesterId == request.OtherUserId && f.AddresseeId == request.CurrentUserId)),
                cancellationToken);

            if (!areFriends)
                return Error.Forbidden(description: "You can only message users on your friends list");

            var existingChannelId = await database.ChannelMembers
                .Where(m => m.UserId == request.CurrentUserId)
                .Select(m => m.ChannelId)
                .Intersect(database.ChannelMembers
                    .Where(m => m.UserId == request.OtherUserId)
                    .Select(m => m.ChannelId))
                .Where(channelId => database.Channels.Any(c => c.Id == channelId && c.Type == ChannelType.DirectMessage))
                .FirstOrDefaultAsync(cancellationToken);

            if (existingChannelId != Guid.Empty)
            {
                var existingChannel = await database.Channels.FirstAsync(c => c.Id == existingChannelId, cancellationToken);
                return ChannelResponse.FromEntity(existingChannel);
            }

            var channel = new Channel
            {
                Id = Guid.NewGuid(),
                Type = ChannelType.DirectMessage,
                Position = 0,
            };
            await database.Channels.AddAsync(channel, cancellationToken);
            await database.ChannelMembers.AddAsync(new ChannelMember { ChannelId = channel.Id, UserId = request.CurrentUserId }, cancellationToken);
            await database.ChannelMembers.AddAsync(new ChannelMember { ChannelId = channel.Id, UserId = request.OtherUserId }, cancellationToken);
            await database.SaveChangesAsync(cancellationToken);

            return ChannelResponse.FromEntity(channel);
        }
    }
}
