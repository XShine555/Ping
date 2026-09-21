namespace Ping.Application.Attachments.Responses
{
    public record AttachmentUploadResponse(string UploadUrl, string Bucket, string Key, DateTime ExpiresAt);
}
