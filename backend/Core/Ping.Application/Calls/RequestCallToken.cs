using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Calls.Responses;
using Ping.Application.Contracts;
using Ping.Application.Shared;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Calls
{
    public record RequestCallTokenCommand(long CurrentUserId, Guid ChannelId) : ICommand<ErrorOr<CallTokenResponse>>;

    public class RequestCallTokenCommandHandler(IDatabase database, ICallTokenService callTokenService)
        : ICommandHandler<RequestCallTokenCommand, ErrorOr<CallTokenResponse>>
    {
        public async ValueTask<ErrorOr<CallTokenResponse>> Handle(RequestCallTokenCommand request, CancellationToken cancellationToken)
        {
            var hasAccess = await database.HasAccessAsync(request.ChannelId, request.CurrentUserId, cancellationToken);
            if (!hasAccess)
                return Error.NotFound(description: $"Channel {request.ChannelId} not found");

            var channel = await database.Channels.AsNoTracking().FirstAsync(c => c.Id == request.ChannelId, cancellationToken);
            if (channel.Type != ChannelType.Voice && channel.Type != ChannelType.DirectMessage)
                return Error.Validation(description: "Calls are only available in voice channels and direct messages");

            var user = await database.Users.AsNoTracking().FirstAsync(u => u.Id == request.CurrentUserId, cancellationToken);

            var accessToken = callTokenService.CreateAccessToken(
                roomName: request.ChannelId.ToString(),
                identity: request.CurrentUserId.ToString(),
                displayName: user.DisplayName ?? user.Username);

            return new CallTokenResponse(accessToken.Jwt, accessToken.RoomName);
        }
    }
}
