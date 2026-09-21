using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Servers.Responses;

namespace Ping.Application.Servers
{
    public record GetUserServersQuery(long UserId) : IQuery<IReadOnlyList<ServerResponse>>;

    public class GetUserServersQueryHandler(IDatabase database)
        : IQueryHandler<GetUserServersQuery, IReadOnlyList<ServerResponse>>
    {
        public async ValueTask<IReadOnlyList<ServerResponse>> Handle(GetUserServersQuery request, CancellationToken cancellationToken)
        {
            var servers = await database.ServerMembers.AsNoTracking()
                .Where(m => m.UserId == request.UserId)
                .Select(m => m.Server!)
                .ToListAsync(cancellationToken);

            return servers.Select(ServerResponse.FromEntity).ToList();
        }
    }
}
