using FluentValidation;
using Ping.Api.DataTransferObjects.Friends;

namespace Ping.Api.Validators.Friends;

public sealed class SendFriendRequestRequestValidator : AbstractValidator<SendFriendRequestRequest>
{
    public SendFriendRequestRequestValidator()
    {
        RuleFor(x => x.AddresseeId)
            .GreaterThan(0);
    }
}
