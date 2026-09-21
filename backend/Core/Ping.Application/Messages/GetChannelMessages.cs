using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Messages.Responses;
using Ping.Application.Shared;

namespace Ping.Application.Messages
{
    public record GetChannelMessagesQuery(long CurrentUserId, Guid ChannelId, Guid? Before, int Take)
        : IQuery<ErrorOr<List<MessageResponse>>>;

    public class GetChannelMessagesQueryHandler(IDatabase database)
        : IQueryHandler<GetChannelMessagesQuery, ErrorOr<List<MessageResponse>>>
    {
        private const int MaxTake = 100;

        public async ValueTask<ErrorOr<List<MessageResponse>>> Handle(GetChannelMessagesQuery request, CancellationToken cancellationToken)
        {
            var hasAccess = await database.HasAccessAsync(request.ChannelId, request.CurrentUserId, cancellationToken);
            if (!hasAccess)
                return Error.NotFound(description: $"Channel {request.ChannelId} not found");

            var take = Math.Clamp(request.Take, 1, MaxTake);

            var query = database.Messages.AsNoTracking()
                .Include(m => m.Author)
                .Include(m => m.Attachments)
                .Where(m => m.ChannelId == request.ChannelId);

            if (request.Before is { } beforeId)
            {
                var cursorCreatedAt = await database.Messages
                    .Where(m => m.Id == beforeId)
                    .Select(m => m.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                query = query.Where(m => m.CreatedAt < cursorCreatedAt);
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);

            return messages.Select(MessageResponse.FromEntity).ToList();
        }
    }
}
