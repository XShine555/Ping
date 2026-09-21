using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ping.Application.Contracts;
using Ping.Application.Users.Responses;
using Ping.Domain.Entities;

namespace Ping.Application.Users
{
    public record SyncUserCommand(long Id, string Username, string? DisplayName, string? AvatarUrl)
        : ICommand<UserResponse>;

    public class SyncUserCommandHandler(IDatabase database, ILogger<SyncUserCommandHandler> logger)
        : ICommandHandler<SyncUserCommand, UserResponse>
    {
        public async ValueTask<UserResponse> Handle(SyncUserCommand request, CancellationToken cancellationToken)
        {
            var user = await database.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user is null)
            {
                user = new User
                {
                    Id = request.Id,
                    Username = request.Username,
                    NormalizedUsername = request.Username.ToUpperInvariant(),
                    DisplayName = request.DisplayName,
                    AvatarUrl = request.AvatarUrl,
                };
                await database.Users.AddAsync(user, cancellationToken);
                logger.LogInformation("Provisioned user {UserId} from identity provider", user.Id);
            }
            else
            {
                user.Username = request.Username;
                user.NormalizedUsername = request.Username.ToUpperInvariant();
                user.DisplayName = request.DisplayName;
                user.AvatarUrl = request.AvatarUrl;
            }

            await database.SaveChangesAsync(cancellationToken);
            return UserResponse.FromEntity(user);
        }
    }
}
