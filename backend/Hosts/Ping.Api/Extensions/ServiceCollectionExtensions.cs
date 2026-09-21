using FluentValidation;
using Ping.Api.Authentication;
using Ping.Api.DataTransferObjects.Servers;
using Ping.Api.Hubs;
using Ping.Api.OpenApi;
using Ping.Api.Scalar;
using Ping.Application;
using Ping.Application.Contracts;
using Ping.Infrastructure.Persistence;
using Ping.Infrastructure.Services;

namespace Ping.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication(configuration);
        services.AddDatabase(configuration);
        services.AddStorageService(configuration);
        services.AddCallTokenService(configuration);
        services.AddAuthenticationConfiguration(configuration);
        services.AddOpenApiConfiguration();
        services.AddScalarConfiguration();
        services.AddValidatorsFromAssemblyContaining<CreateServerRequest>();

        services.AddSignalR();
        services.AddScoped<IRealtimeNotifier, RealtimeNotifier>();
        services.AddSingleton<PresenceTracker>();
        services.AddSingleton<IPresenceTracker>(sp => sp.GetRequiredService<PresenceTracker>());

        return services;
    }
}
