using Ping.Domain.ValueObjects;

namespace Ping.Api.DataTransferObjects.Messages;

public record MessageAttachmentRequest(string Key, string FileName, string ContentType, AttachmentKind Kind);

public record SendMessageRequest(string? Content, Guid? ReplyToMessageId, IReadOnlyList<MessageAttachmentRequest>? Attachments);
