using FluentValidation;
using Ping.Api.DataTransferObjects.Messages;

namespace Ping.Api.Validators.Messages;

public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .MaximumLength(4000);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || (x.Attachments?.Count ?? 0) > 0)
            .WithMessage("A message needs content or at least one attachment");
    }
}
