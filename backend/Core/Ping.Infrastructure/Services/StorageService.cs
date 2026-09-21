using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Ping.Application.Contracts;
using Ping.Infrastructure.Configuration;

namespace Ping.Infrastructure.Services
{
    public class StorageService(IAmazonS3 amazonS3, ILogger<StorageService> logger, InfrastructureStorageConfiguration storageClientConfiguration)
        : IStorageService
    {
        public async Task<Stream> GetFileAsync(string bucket, string key, CancellationToken cancellationToken)
        {
            var request = new GetObjectRequest { BucketName = bucket, Key = key };

            try
            {
                var response = await amazonS3.GetObjectAsync(request, cancellationToken);
                logger.LogDebug("Retrieved file from S3 {Bucket}/{Key}", bucket, key);
                return response.ResponseStream;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to get file from S3 {Bucket}/{Key}", bucket, key);
                throw;
            }
        }

        public async Task<string> GetUrlAsync(string bucket, string key, TimeSpan expirationTime, CancellationToken cancellationToken)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = key,
                Expires = DateTime.UtcNow + expirationTime,
                Protocol = storageClientConfiguration.UseHttp ? Protocol.HTTP : Protocol.HTTPS,
            };

            logger.LogDebug("Generating pre-signed URL for {Bucket}/{Key}", bucket, key);
            return await amazonS3.GetPreSignedURLAsync(request);
        }

        public async Task<string> GetUploadUrlAsync(
            string bucket,
            string key,
            string contentType,
            TimeSpan expirationTime,
            CancellationToken cancellationToken,
            bool preventOverwrite = true)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = key,
                Expires = DateTime.UtcNow + expirationTime,
                Protocol = storageClientConfiguration.UseHttp ? Protocol.HTTP : Protocol.HTTPS,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
            };

            if (preventOverwrite)
                request.Headers["If-None-Match"] = "*";

            logger.LogDebug("Generating pre-signed upload URL for {Bucket}/{Key}", bucket, key);
            return await amazonS3.GetPreSignedURLAsync(request);
        }

        public async Task<ObjectMetaData?> HeadObjectAsync(string bucket, string key, CancellationToken cancellationToken)
        {
            try
            {
                var response = await amazonS3.GetObjectMetadataAsync(bucket, key, cancellationToken);
                return new ObjectMetaData(response.Headers.ContentType, response.ContentLength);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "HEAD failed for {Bucket}/{Key}", bucket, key);
                throw;
            }
        }

        public async Task RemoveFileAsync(string bucket, string key, CancellationToken cancellationToken)
        {
            var request = new DeleteObjectRequest { BucketName = bucket, Key = key };

            logger.LogDebug("Removing file from S3 {Bucket}/{Key}", bucket, key);
            await amazonS3.DeleteObjectAsync(request, cancellationToken);
        }
    }
}
