using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Channels.Responses;
using Ping.Application.Contracts;

namespace Ping.Application.Channels
{
    public record GetServerChannelsQuery(long CurrentUserId, Guid ServerId) : IQuery<ErrorOr<List<ChannelResponse>>>;

    public class GetServerChannelsQueryHandler(IDatabase database)
        : IQueryHandler<GetServerChannelsQuery, ErrorOr<List<ChannelResponse>>>
    {
        public async ValueTask<ErrorOr<List<ChannelResponse>>> Handle(GetServerChannelsQuery request, CancellationToken cancellationToken)
        {
            var isMember = await database.ServerMembers.AnyAsync(
                m => m.ServerId == request.ServerId && m.UserId == request.CurrentUserId, cancellationToken);

            if (!isMember)
                return Error.NotFound(description: $"Server {request.ServerId} not found");

            var channels = await database.Channels.AsNoTracking()
                .Where(c => c.ServerId == request.ServerId)
                .OrderBy(c => c.Position)
                .ToListAsync(cancellationToken);

            return channels.Select(ChannelResponse.FromEntity).ToList();
        }
    }
}
