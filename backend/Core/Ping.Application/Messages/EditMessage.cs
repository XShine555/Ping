using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Messages.Responses;

namespace Ping.Application.Messages
{
    public record EditMessageCommand(long CurrentUserId, Guid MessageId, string Content) : ICommand<ErrorOr<MessageResponse>>;

    public class EditMessageCommandHandler(IDatabase database, IRealtimeNotifier notifier)
        : ICommandHandler<EditMessageCommand, ErrorOr<MessageResponse>>
    {
        public async ValueTask<ErrorOr<MessageResponse>> Handle(EditMessageCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return Error.Validation(description: "Message content cannot be empty");

            var message = await database.Messages
                .Include(m => m.Author)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == request.MessageId && m.DeletedAt == null, cancellationToken);

            if (message is null)
                return Error.NotFound(description: $"Message {request.MessageId} not found");

            if (message.AuthorId != request.CurrentUserId)
                return Error.Forbidden(description: "Only the author can edit this message");

            message.Content = request.Content;
            message.EditedAt = DateTime.UtcNow;
            await database.SaveChangesAsync(cancellationToken);

            var response = MessageResponse.FromEntity(message);
            await notifier.MessageUpdatedAsync(message.ChannelId, response, cancellationToken);
            return response;
        }
    }
}
