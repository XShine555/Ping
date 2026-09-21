using Mediator;
using Ping.Api.Authentication;
using Ping.Api.DataTransferObjects.Messages;
using Ping.Api.Extensions;
using Ping.Api.Filters;
using Ping.Application.Messages;
using Ping.Application.Messages.Responses;

namespace Ping.Api.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var channelMessages = app.MapGroup("/channels/{channelId:guid}/messages")
            .WithTags("Messages")
            .RequireAuthorization();

        channelMessages.MapGet("/", GetChannelMessages)
            .WithName("GetChannelMessages")
            .WithSummary("Get A Channel'S Messages, Newest First, Optionally Before A Given Message.")
            .Produces<IEnumerable<MessageResponse>>()
            .Produces(StatusCodes.Status404NotFound);

        channelMessages.MapPost("/", SendMessage)
            .WithName("SendMessage")
            .WithSummary("Send A Message To A Channel.")
            .AddEndpointFilter<ValidationFilter<SendMessageRequest>>()
            .Produces<MessageResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);

        var messages = app.MapGroup("/messages")
            .WithTags("Messages")
            .RequireAuthorization();

        messages.MapPatch("/{id:guid}", EditMessage)
            .WithName("EditMessage")
            .WithSummary("Edit A Message'S Content.")
            .AddEndpointFilter<ValidationFilter<EditMessageRequest>>()
            .Produces<MessageResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        messages.MapDelete("/{id:guid}", DeleteMessage)
            .WithName("DeleteMessage")
            .WithSummary("Delete A Message (Soft Delete).")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetChannelMessages(
        IMediator mediator, CurrentUser currentUser, Guid channelId, Guid? before, int take, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetChannelMessagesQuery(currentUser.RequiredId, channelId, before, take == 0 ? 50 : take), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> SendMessage(
        IMediator mediator, CurrentUser currentUser, Guid channelId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var attachments = (request.Attachments ?? [])
            .Select(a => new PendingAttachment(a.Key, a.FileName, a.ContentType, a.Kind))
            .ToList();

        var result = await mediator.Send(
            new SendMessageCommand(currentUser.RequiredId, channelId, request.Content ?? string.Empty, request.ReplyToMessageId, attachments),
            cancellationToken);

        return result.ToCreatedResult(message => $"/channels/{channelId}/messages/{message.Id}");
    }

    private static async Task<IResult> EditMessage(
        IMediator mediator, CurrentUser currentUser, Guid id, EditMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new EditMessageCommand(currentUser.RequiredId, id, request.Content), cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteMessage(
        IMediator mediator, CurrentUser currentUser, Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteMessageCommand(currentUser.RequiredId, id), cancellationToken);
        return result.ToHttpResult();
    }
}
