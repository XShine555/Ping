using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Messages.Responses
{
    public record AttachmentResponse(Guid Id, string FileName, string ContentType, long SizeBytes, AttachmentKind Kind)
    {
        public static AttachmentResponse FromEntity(Attachment attachment) =>
            new(attachment.Id, attachment.FileName, attachment.ContentType, attachment.SizeBytes, attachment.Kind);
    }
}
