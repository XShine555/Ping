using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ping.Application.Contracts;
using Ping.Infrastructure.Configuration;

namespace Ping.Infrastructure.Persistence
{
    public static class DatabaseDependencyInjection
    {
        public static IServiceCollection AddDatabase(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors
                .AddOptionsWithValidateOnStart<DatabaseConfiguration>()
                .Bind(configuration.GetRequiredSection(DatabaseConfiguration.SectionName))
                .ValidateDataAnnotations();

            serviceDescriptors.AddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<DatabaseConfiguration>>().Value);

            serviceDescriptors.AddSingleton<AuditableEntityInterceptor>();

            serviceDescriptors.AddDbContext<Database>((serviceProvider, options) =>
                options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

            serviceDescriptors.AddScoped<IDatabase>(serviceProvider => serviceProvider.GetRequiredService<Database>());

            return serviceDescriptors;
        }
    }
}
