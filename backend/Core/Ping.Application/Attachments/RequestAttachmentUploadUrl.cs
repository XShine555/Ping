using Mediator;
using Ping.Application.Attachments.Responses;
using Ping.Application.Configuration;
using Ping.Application.Contracts;

namespace Ping.Application.Attachments
{
    public record RequestAttachmentUploadUrlCommand(long RequesterId, string FileName, string ContentType)
        : ICommand<AttachmentUploadResponse>;

    public class RequestAttachmentUploadUrlCommandHandler(IStorageService storageService, ApplicationStorageConfiguration storageConfiguration)
        : ICommandHandler<RequestAttachmentUploadUrlCommand, AttachmentUploadResponse>
    {
        private static readonly TimeSpan UploadUrlExpiration = TimeSpan.FromMinutes(10);

        public async ValueTask<AttachmentUploadResponse> Handle(RequestAttachmentUploadUrlCommand request, CancellationToken cancellationToken)
        {
            var key = $"attachments/{request.RequesterId}/{Guid.NewGuid()}/{request.FileName}";
            var expiresAt = DateTime.UtcNow + UploadUrlExpiration;

            var uploadUrl = await storageService.GetUploadUrlAsync(
                storageConfiguration.Bucket, key, request.ContentType, UploadUrlExpiration, cancellationToken);

            return new AttachmentUploadResponse(uploadUrl, storageConfiguration.Bucket, key, expiresAt);
        }
    }
}
