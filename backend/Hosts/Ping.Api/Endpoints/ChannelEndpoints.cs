using Mediator;
using Ping.Api.Authentication;
using Ping.Api.DataTransferObjects.Channels;
using Ping.Api.Extensions;
using Ping.Api.Filters;
using Ping.Application.Channels;
using Ping.Application.Channels.Responses;

namespace Ping.Api.Endpoints;

public static class ChannelEndpoints
{
    public static IEndpointRouteBuilder MapChannelEndpoints(this IEndpointRouteBuilder app)
    {
        var servers = app.MapGroup("/servers/{serverId:guid}/channels")
            .WithTags("Channels")
            .RequireAuthorization();

        servers.MapGet("/", GetServerChannels)
            .WithName("GetServerChannels")
            .WithSummary("Get A Server'S Channels.")
            .Produces<IEnumerable<ChannelResponse>>()
            .Produces(StatusCodes.Status404NotFound);

        servers.MapPost("/", CreateChannel)
            .WithName("CreateChannel")
            .WithSummary("Create A Text Or Voice Channel In A Server.")
            .AddEndpointFilter<ValidationFilter<CreateChannelRequest>>()
            .Produces<ChannelResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        var directMessages = app.MapGroup("/channels/dm")
            .WithTags("Channels")
            .RequireAuthorization();

        directMessages.MapPost("/{otherUserId:long}", GetOrCreateDirectMessageChannel)
            .WithName("GetOrCreateDirectMessageChannel")
            .WithSummary("Get Or Create A Direct Message Channel With A Friend.")
            .Produces<ChannelResponse>()
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> GetServerChannels(
        IMediator mediator, CurrentUser currentUser, Guid serverId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetServerChannelsQuery(currentUser.RequiredId, serverId), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateChannel(
        IMediator mediator, CurrentUser currentUser, Guid serverId, CreateChannelRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateChannelCommand(currentUser.RequiredId, serverId, request.Name, request.Type), cancellationToken);
        return result.ToCreatedResult(channel => $"/servers/{serverId}/channels/{channel.Id}");
    }

    private static async Task<IResult> GetOrCreateDirectMessageChannel(
        IMediator mediator, CurrentUser currentUser, long otherUserId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetOrCreateDirectMessageChannelCommand(currentUser.RequiredId, otherUserId), cancellationToken);
        return result.ToHttpResult();
    }
}
