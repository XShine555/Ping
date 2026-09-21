using Ping.Application.Users.Responses;
using Ping.Domain.Entities;

namespace Ping.Application.Messages.Responses
{
    public record MessageResponse(
        Guid Id,
        Guid ChannelId,
        UserResponse Author,
        string Content,
        Guid? ReplyToMessageId,
        DateTime CreatedAt,
        DateTime? EditedAt,
        IReadOnlyList<AttachmentResponse> Attachments)
    {
        public static MessageResponse FromEntity(Message message) =>
            new(
                message.Id,
                message.ChannelId,
                UserResponse.FromEntity(message.Author!),
                message.Content,
                message.ReplyToMessageId,
                message.CreatedAt,
                message.EditedAt,
                message.Attachments.Select(AttachmentResponse.FromEntity).ToList());
    }
}
