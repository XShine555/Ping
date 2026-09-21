using FluentValidation;
using Ping.Api.DataTransferObjects.Attachments;

namespace Ping.Api.Validators.Attachments;

public sealed class RequestAttachmentUploadUrlRequestValidator : AbstractValidator<RequestAttachmentUploadUrlRequest>
{
    public RequestAttachmentUploadUrlRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(255);
    }
}
