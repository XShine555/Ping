using System.Text.Json.Serialization;
using Ping.Application.Serialization;
using Ping.Domain.Entities;

namespace Ping.Application.Servers.Responses
{
    public record ServerResponse(Guid Id, string Name, string? IconUrl, [property: JsonConverter(typeof(LongAsStringConverter))] long OwnerId)
    {
        public static ServerResponse FromEntity(Server server) =>
            new(server.Id, server.Name, server.IconUrl, server.OwnerId);
    }
}
