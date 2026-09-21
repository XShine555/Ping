using Mediator;
using Ping.Application.Contracts;
using Ping.Application.Servers.Responses;
using Ping.Domain.Entities;
using Ping.Domain.ValueObjects;

namespace Ping.Application.Servers
{
    public record CreateServerCommand(long OwnerId, string Name) : ICommand<ServerResponse>;

    public class CreateServerCommandHandler(IDatabase database)
        : ICommandHandler<CreateServerCommand, ServerResponse>
    {
        public async ValueTask<ServerResponse> Handle(CreateServerCommand request, CancellationToken cancellationToken)
        {
            var server = new Server
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                OwnerId = request.OwnerId,
            };
            await database.Servers.AddAsync(server, cancellationToken);

            await database.ServerMembers.AddAsync(new ServerMember
            {
                ServerId = server.Id,
                UserId = request.OwnerId,
                Role = ServerMemberRole.Owner,
            }, cancellationToken);

            await database.Channels.AddAsync(new Channel
            {
                Id = Guid.NewGuid(),
                ServerId = server.Id,
                Name = "general",
                Type = ChannelType.Text,
                Position = 0,
            }, cancellationToken);

            await database.SaveChangesAsync(cancellationToken);
            return ServerResponse.FromEntity(server);
        }
    }
}
