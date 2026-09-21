using Mediator;
using Ping.Api.Authentication;
using Ping.Api.DataTransferObjects.Friends;
using Ping.Api.Extensions;
using Ping.Api.Filters;
using Ping.Application.Friends;
using Ping.Application.Friends.Responses;

namespace Ping.Api.Endpoints;

public static class FriendEndpoints
{
    public static IEndpointRouteBuilder MapFriendEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/friends")
            .WithTags("Friends")
            .RequireAuthorization();

        group.MapGet("/", GetFriends)
            .WithName("GetFriends")
            .WithSummary("Get The Current User'S Accepted Friends.")
            .Produces<IEnumerable<FriendshipResponse>>();

        group.MapGet("/requests", GetPendingFriendRequests)
            .WithName("GetPendingFriendRequests")
            .WithSummary("Get Incoming And Outgoing Pending Friend Requests.")
            .Produces<IEnumerable<FriendshipResponse>>();

        group.MapPost("/requests", SendFriendRequest)
            .WithName("SendFriendRequest")
            .WithSummary("Send A Friend Request.")
            .AddEndpointFilter<ValidationFilter<SendFriendRequestRequest>>()
            .Produces<FriendshipResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/requests/{id:guid}/accept", AcceptFriendRequest)
            .WithName("AcceptFriendRequest")
            .WithSummary("Accept A Pending Friend Request.")
            .Produces<FriendshipResponse>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/requests/{id:guid}/reject", RejectFriendRequest)
            .WithName("RejectFriendRequest")
            .WithSummary("Reject A Pending Friend Request.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", RemoveFriend)
            .WithName("RemoveFriend")
            .WithSummary("Remove An Existing Friend.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/block/{userId:long}", BlockUser)
            .WithName("BlockUser")
            .WithSummary("Block A User.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IEnumerable<FriendshipResponse>> GetFriends(
        IMediator mediator, CurrentUser currentUser, CancellationToken cancellationToken) =>
        await mediator.Send(new GetFriendsQuery(currentUser.RequiredId), cancellationToken);

    private static async Task<IEnumerable<FriendshipResponse>> GetPendingFriendRequests(
        IMediator mediator, CurrentUser currentUser, CancellationToken cancellationToken) =>
        await mediator.Send(new GetPendingFriendRequestsQuery(currentUser.RequiredId), cancellationToken);

    private static async Task<IResult> SendFriendRequest(
        IMediator mediator, CurrentUser currentUser, SendFriendRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SendFriendRequestCommand(currentUser.RequiredId, request.AddresseeId), cancellationToken);
        return result.ToCreatedResult(friendship => $"/friends/requests/{friendship.Id}");
    }

    private static async Task<IResult> AcceptFriendRequest(
        IMediator mediator, CurrentUser currentUser, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AcceptFriendRequestCommand(currentUser.RequiredId, id), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RejectFriendRequest(
        IMediator mediator, CurrentUser currentUser, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RejectFriendRequestCommand(currentUser.RequiredId, id), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RemoveFriend(
        IMediator mediator, CurrentUser currentUser, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveFriendCommand(currentUser.RequiredId, id), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> BlockUser(
        IMediator mediator, CurrentUser currentUser, long userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new BlockUserCommand(currentUser.RequiredId, userId), cancellationToken);
        return result.ToHttpResult();
    }
}
