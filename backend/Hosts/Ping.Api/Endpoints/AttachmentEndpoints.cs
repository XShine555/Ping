using Mediator;
using Ping.Api.Authentication;
using Ping.Api.DataTransferObjects.Attachments;
using Ping.Api.Filters;
using Ping.Application.Attachments;
using Ping.Application.Attachments.Responses;

namespace Ping.Api.Endpoints;

public static class AttachmentEndpoints
{
    public static IEndpointRouteBuilder MapAttachmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/attachments")
            .WithTags("Attachments")
            .RequireAuthorization();

        group.MapPost("/upload-url", RequestUploadUrl)
            .WithName("RequestAttachmentUploadUrl")
            .WithSummary("Request A Presigned Url To Upload A File Or Voice Message Directly To Storage.")
            .AddEndpointFilter<ValidationFilter<RequestAttachmentUploadUrlRequest>>()
            .Produces<AttachmentUploadResponse>()
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<AttachmentUploadResponse> RequestUploadUrl(
        IMediator mediator, CurrentUser currentUser, RequestAttachmentUploadUrlRequest request, CancellationToken cancellationToken) =>
        await mediator.Send(new RequestAttachmentUploadUrlCommand(currentUser.RequiredId, request.FileName, request.ContentType), cancellationToken);
}
