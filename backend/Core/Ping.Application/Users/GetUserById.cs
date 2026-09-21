using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Ping.Application.Contracts;
using Ping.Application.Users.Responses;

namespace Ping.Application.Users
{
    public record GetUserByIdQuery(long Id) : IQuery<ErrorOr<UserResponse>>;

    public class GetUserByIdQueryHandler(IDatabase database)
        : IQueryHandler<GetUserByIdQuery, ErrorOr<UserResponse>>
    {
        public async ValueTask<ErrorOr<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await database.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            return user is null
                ? Error.NotFound(description: $"User {request.Id} not found")
                : UserResponse.FromEntity(user);
        }
    }
}
