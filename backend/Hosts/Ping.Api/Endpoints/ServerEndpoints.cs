using Mediator;
using Ping.Api.Authentication;
using Ping.Api.DataTransferObjects.Servers;
using Ping.Api.Filters;
using Ping.Application.Servers;
using Ping.Application.Servers.Responses;

namespace Ping.Api.Endpoints;

public static class ServerEndpoints
{
    public static IEndpointRouteBuilder MapServerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/servers")
            .WithTags("Servers")
            .RequireAuthorization();

        group.MapGet("/", GetUserServers)
            .WithName("GetUserServers")
            .WithSummary("Get The Servers The Current User Is A Member Of.")
            .Produces<IEnumerable<ServerResponse>>();

        group.MapPost("/", CreateServer)
            .WithName("CreateServer")
            .WithSummary("Create A New Server, With The Current User As Owner.")
            .AddEndpointFilter<ValidationFilter<CreateServerRequest>>()
            .Produces<ServerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IEnumerable<ServerResponse>> GetUserServers(
        IMediator mediator, CurrentUser currentUser, CancellationToken cancellationToken) =>
        await mediator.Send(new GetUserServersQuery(currentUser.RequiredId), cancellationToken);

    private static async Task<IResult> CreateServer(
        IMediator mediator, CurrentUser currentUser, CreateServerRequest request, CancellationToken cancellationToken)
    {
        var server = await mediator.Send(new CreateServerCommand(currentUser.RequiredId, request.Name), cancellationToken);
        return Results.Created($"/servers/{server.Id}", server);
    }
}
