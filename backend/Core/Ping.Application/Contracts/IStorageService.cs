namespace Ping.Application.Contracts
{
    public interface IStorageService
    {
        Task<Stream> GetFileAsync(string bucket, string key, CancellationToken cancellationToken);

        Task<string> GetUrlAsync(string bucket, string key, TimeSpan expirationTime, CancellationToken cancellationToken);

        Task<string> GetUploadUrlAsync(
            string bucket,
            string key,
            string contentType,
            TimeSpan expirationTime,
            CancellationToken cancellationToken,
            bool preventOverwrite = true);

        Task<ObjectMetaData?> HeadObjectAsync(string bucket, string key, CancellationToken cancellationToken);

        Task RemoveFileAsync(string bucket, string key, CancellationToken cancellationToken);
    }
}
