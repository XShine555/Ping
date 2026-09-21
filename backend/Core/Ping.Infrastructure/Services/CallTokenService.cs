using Livekit.Server.Sdk.Dotnet;
using Ping.Application.Contracts;
using Ping.Infrastructure.Configuration;

namespace Ping.Infrastructure.Services
{
    public class CallTokenService(LiveKitConfiguration configuration) : ICallTokenService
    {
        public CallAccessToken CreateAccessToken(string roomName, string identity, string? displayName)
        {
            var token = new AccessToken(configuration.ApiKey, configuration.ApiSecret)
                .WithIdentity(identity)
                .WithName(displayName ?? identity)
                .WithGrants(new VideoGrants
                {
                    RoomJoin = true,
                    Room = roomName,
                    CanPublish = true,
                    CanSubscribe = true,
                    CanPublishData = true,
                })
                .WithTtl(TimeSpan.FromMinutes(configuration.TokenTtlMinutes));

            return new CallAccessToken(token.ToJwt(), roomName);
        }
    }
}
