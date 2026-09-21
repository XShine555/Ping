using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ping.Application.Contracts;
using Ping.Infrastructure.Configuration;

namespace Ping.Infrastructure.Services
{
    public static class ServicesDependencyInjection
    {
        public static IServiceCollection AddStorageService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors
                .AddOptionsWithValidateOnStart<InfrastructureStorageConfiguration>()
                .Bind(configuration.GetRequiredSection(InfrastructureStorageConfiguration.SectionName))
                .ValidateDataAnnotations();

            serviceDescriptors.AddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<InfrastructureStorageConfiguration>>().Value);

            serviceDescriptors.AddSingleton<IAmazonS3>(serviceProvider =>
            {
                var storageConfiguration = serviceProvider.GetRequiredService<InfrastructureStorageConfiguration>();
                var s3Configuration = new AmazonS3Config
                {
                    ServiceURL = storageConfiguration.Address,
                    ForcePathStyle = storageConfiguration.ForcePathStyle,
                    UseHttp = storageConfiguration.UseHttp,
                };
                return new AmazonS3Client(storageConfiguration.AccessKey, storageConfiguration.SecretAccessKey, s3Configuration);
            });

            serviceDescriptors.AddScoped<IStorageService, StorageService>();
            return serviceDescriptors;
        }

        public static IServiceCollection AddCallTokenService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors
                .AddOptionsWithValidateOnStart<LiveKitConfiguration>()
                .Bind(configuration.GetRequiredSection(LiveKitConfiguration.SectionName))
                .ValidateDataAnnotations();

            serviceDescriptors.AddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<LiveKitConfiguration>>().Value);

            serviceDescriptors.AddSingleton<ICallTokenService, CallTokenService>();
            return serviceDescriptors;
        }
    }
}
