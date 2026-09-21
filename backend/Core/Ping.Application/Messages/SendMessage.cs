using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Configuration;
using Ping.Application.Contracts;
using Ping.Application.Messages.Responses;
using Ping.Application.Shared;
using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Messages
{
    public record PendingAttachment(string Key, string FileName, string ContentType, AttachmentKind Kind);

    public record SendMessageCommand(
        long AuthorId,
        Guid ChannelId,
        string Content,
        Guid? ReplyToMessageId,
        IReadOnlyList<PendingAttachment> Attachments)
        : ICommand<ErrorOr<MessageResponse>>;

    public class SendMessageCommandHandler(
        IDatabase database,
        IStorageService storageService,
        ApplicationStorageConfiguration storageConfiguration,
        IRealtimeNotifier notifier)
        : ICommandHandler<SendMessageCommand, ErrorOr<MessageResponse>>
    {
        public async ValueTask<ErrorOr<MessageResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Content) && request.Attachments.Count == 0)
                return Error.Validation(description: "A message needs content or at least one attachment");

            var hasAccess = await database.HasAccessAsync(request.ChannelId, request.AuthorId, cancellationToken);
            if (!hasAccess)
                return Error.NotFound(description: $"Channel {request.ChannelId} not found");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChannelId = request.ChannelId,
                AuthorId = request.AuthorId,
                Content = request.Content,
                ReplyToMessageId = request.ReplyToMessageId,
            };

            foreach (var pending in request.Attachments)
            {
                var metadata = await storageService.HeadObjectAsync(storageConfiguration.Bucket, pending.Key, cancellationToken);
                if (metadata is null)
                    return Error.Validation(description: $"Attachment '{pending.FileName}' was not found in storage");

                message.Attachments.Add(new Attachment
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    Bucket = storageConfiguration.Bucket,
                    Key = pending.Key,
                    FileName = pending.FileName,
                    ContentType = pending.ContentType,
                    SizeBytes = metadata.ContentLength,
                    Kind = pending.Kind,
                });
            }

            await database.Messages.AddAsync(message, cancellationToken);
            await database.SaveChangesAsync(cancellationToken);

            message.Author = await database.Users.FirstAsync(u => u.Id == request.AuthorId, cancellationToken);
            var response = MessageResponse.FromEntity(message);

            await notifier.MessageCreatedAsync(request.ChannelId, response, cancellationToken);
            return response;
        }
    }
}
