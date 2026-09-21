using Mediator;
using Ping.Api.Authentication;
using Ping.Api.Extensions;
using Ping.Application.Users;

namespace Ping.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get The Currently Authenticated User.")
            .Produces<Application.Users.Responses.UserResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:long}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Get A User By Id.")
            .Produces<Application.Users.Responses.UserResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/search", SearchUsers)
            .WithName("SearchUsers")
            .WithSummary("Search Users By Username, Excluding The Current User.")
            .Produces<IEnumerable<Application.Users.Responses.UserResponse>>();

        return app;
    }

    private static async Task<IResult> GetCurrentUser(IMediator mediator, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(currentUser.RequiredId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetUserById(IMediator mediator, long id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IEnumerable<Application.Users.Responses.UserResponse>> SearchUsers(
        IMediator mediator, CurrentUser currentUser, string query, CancellationToken cancellationToken) =>
        await mediator.Send(new SearchUsersQuery(query, currentUser.RequiredId), cancellationToken);
}
