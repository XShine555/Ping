using FluentValidation;
using Ping.Api.DataTransferObjects.Channels;
using Ping.Domain.ValueObjects;

namespace Ping.Api.Validators.Channels;

public sealed class CreateChannelRequestValidator : AbstractValidator<CreateChannelRequest>
{
    public CreateChannelRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .IsInEnum()
            .NotEqual(ChannelType.DirectMessage);
    }
}
