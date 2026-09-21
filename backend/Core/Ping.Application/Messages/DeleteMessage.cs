using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;

namespace Ping.Application.Messages
{
    public record DeleteMessageCommand(long CurrentUserId, Guid MessageId) : ICommand<ErrorOr<Success>>;

    public class DeleteMessageCommandHandler(IDatabase database, IRealtimeNotifier notifier)
        : ICommandHandler<DeleteMessageCommand, ErrorOr<Success>>
    {
        public async ValueTask<ErrorOr<Success>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await database.Messages
                .FirstOrDefaultAsync(m => m.Id == request.MessageId && m.DeletedAt == null, cancellationToken);

            if (message is null)
                return Error.NotFound(description: $"Message {request.MessageId} not found");

            if (message.AuthorId != request.CurrentUserId)
            {
                var isServerOwner = await database.Channels
                    .Where(c => c.Id == message.ChannelId && c.ServerId != null)
                    .Select(c => c.ServerId)
                    .Join(database.Servers, serverId => serverId, server => server.Id, (_, server) => server.OwnerId)
                    .AnyAsync(ownerId => ownerId == request.CurrentUserId, cancellationToken);

                if (!isServerOwner)
                    return Error.Forbidden(description: "Only the author or the server owner can delete this message");
            }

            message.DeletedAt = DateTime.UtcNow;
            message.Content = string.Empty;
            await database.SaveChangesAsync(cancellationToken);

            await notifier.MessageDeletedAsync(message.ChannelId, message.Id, cancellationToken);
            return new Success();
        }
    }
}
