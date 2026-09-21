using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Channels.Responses;
using Ping.Application.Contracts;
using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Channels
{
    public record CreateChannelCommand(long CurrentUserId, Guid ServerId, string Name, ChannelType Type)
        : ICommand<ErrorOr<ChannelResponse>>;

    public class CreateChannelCommandHandler(IDatabase database)
        : ICommandHandler<CreateChannelCommand, ErrorOr<ChannelResponse>>
    {
        public async ValueTask<ErrorOr<ChannelResponse>> Handle(CreateChannelCommand request, CancellationToken cancellationToken)
        {
            var membership = await database.ServerMembers.FirstOrDefaultAsync(
                m => m.ServerId == request.ServerId && m.UserId == request.CurrentUserId, cancellationToken);

            if (membership is null)
                return Error.NotFound(description: $"Server {request.ServerId} not found");

            if (membership.Role != ServerMemberRole.Owner)
                return Error.Forbidden(description: "Only the server owner can create channels");

            if (request.Type == ChannelType.DirectMessage)
                return Error.Validation(description: "Server channels cannot be direct messages");

            var position = await database.Channels
                .Where(c => c.ServerId == request.ServerId)
                .CountAsync(cancellationToken);

            var channel = new Channel
            {
                Id = Guid.NewGuid(),
                ServerId = request.ServerId,
                Name = request.Name,
                Type = request.Type,
                Position = position,
            };
            await database.Channels.AddAsync(channel, cancellationToken);
            await database.SaveChangesAsync(cancellationToken);

            return ChannelResponse.FromEntity(channel);
        }
    }
}
