using Academy.Application.Contracts.Auth;
using FluentValidation;

namespace Academy.Application.Validation.Auth;

public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}