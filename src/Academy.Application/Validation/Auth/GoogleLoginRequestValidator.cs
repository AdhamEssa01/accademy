using Academy.Application.Contracts.Auth;
using FluentValidation;

namespace Academy.Application.Validation.Auth;

public sealed class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .MinimumLength(10);
    }
}