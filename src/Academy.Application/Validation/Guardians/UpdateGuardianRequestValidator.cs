using Academy.Application.Contracts.Guardians;
using FluentValidation;

namespace Academy.Application.Validation.Guardians;

public sealed class UpdateGuardianRequestValidator : AbstractValidator<UpdateGuardianRequest>
{
    public UpdateGuardianRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .MaximumLength(254)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
