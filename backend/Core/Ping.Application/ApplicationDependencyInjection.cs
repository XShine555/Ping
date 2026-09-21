using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ping.Application.Configuration;

namespace Ping.Application
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

            services.AddValidatedOptions<ApplicationStorageConfiguration>(configuration, ApplicationStorageConfiguration.SectionName);

            return services;
        }
    }
}
