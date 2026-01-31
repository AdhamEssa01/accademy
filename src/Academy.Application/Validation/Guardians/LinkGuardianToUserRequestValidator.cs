using Academy.Application.Contracts.Guardians;
using FluentValidation;

namespace Academy.Application.Validation.Guardians;

public sealed class LinkGuardianToUserRequestValidator : AbstractValidator<LinkGuardianToUserRequest>
{
    public LinkGuardianToUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
