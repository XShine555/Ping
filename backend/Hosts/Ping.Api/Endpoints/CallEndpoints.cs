using Mediator;
using Ping.Api.Authentication;
using Ping.Api.Extensions;
using Ping.Application.Calls;

namespace Ping.Api.Endpoints;

public static class CallEndpoints
{
    public static IEndpointRouteBuilder MapCallEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/channels/{channelId:guid}/call-token")
            .WithTags("Calls")
            .RequireAuthorization();

        group.MapPost("/", RequestCallToken)
            .WithName("RequestCallToken")
            .WithSummary("Request A Livekit Access Token To Join The Voice/Video Call For A Channel.")
            .Produces<Application.Calls.Responses.CallTokenResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RequestCallToken(
        IMediator mediator, CurrentUser currentUser, Guid channelId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RequestCallTokenCommand(currentUser.RequiredId, channelId), cancellationToken);
        return result.ToHttpResult();
    }
}
