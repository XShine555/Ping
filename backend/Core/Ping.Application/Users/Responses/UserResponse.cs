using System.Text.Json.Serialization;
using Ping.Application.Serialization;
using Ping.Domain.Entities;

namespace Ping.Application.Users.Responses
{
    public record UserResponse(
        [property: JsonConverter(typeof(LongAsStringConverter))] long Id,
        string Username,
        string? DisplayName,
        string? AvatarUrl)
    {
        public static UserResponse FromEntity(User user) =>
            new(user.Id, user.Username, user.DisplayName, user.AvatarUrl);
    }
}
