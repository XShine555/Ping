using FluentValidation;
using Ping.Api.DataTransferObjects.Servers;

namespace Ping.Api.Validators.Servers;

public sealed class CreateServerRequestValidator : AbstractValidator<CreateServerRequest>
{
    public CreateServerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
