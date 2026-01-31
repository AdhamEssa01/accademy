using Academy.Application.Contracts.Guardians;
using FluentValidation;

namespace Academy.Application.Validation.Guardians;

public sealed class LinkGuardianToStudentRequestValidator : AbstractValidator<LinkGuardianToStudentRequest>
{
    public LinkGuardianToStudentRequestValidator()
    {
        RuleFor(x => x.Relation)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Relation));
    }
}
