using FluentValidation;
using Ping.Api.DataTransferObjects.Messages;

namespace Ping.Api.Validators.Messages;

public sealed class EditMessageRequestValidator : AbstractValidator<EditMessageRequest>
{
    public EditMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
