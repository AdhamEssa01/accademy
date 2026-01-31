using Academy.Application.Contracts.Auth;
using FluentValidation;

namespace Academy.Application.Validation.Auth;

public sealed class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}