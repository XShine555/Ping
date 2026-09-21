using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Users.Responses;

namespace Ping.Application.Users
{
    public record SearchUsersQuery(string UsernameQuery, long ExcludingUserId) : IQuery<IReadOnlyList<UserResponse>>;

    public class SearchUsersQueryHandler(IDatabase database)
        : IQueryHandler<SearchUsersQuery, IReadOnlyList<UserResponse>>
    {
        public async ValueTask<IReadOnlyList<UserResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            var normalizedQuery = request.UsernameQuery.Trim().ToUpperInvariant();
            if (normalizedQuery.Length == 0)
                return [];

            var users = await database.Users.AsNoTracking()
                .Where(u => u.Id != request.ExcludingUserId && u.NormalizedUsername.Contains(normalizedQuery))
                .OrderBy(u => u.Username)
                .Take(20)
                .ToListAsync(cancellationToken);

            return users.Select(UserResponse.FromEntity).ToList();
        }
    }
}
