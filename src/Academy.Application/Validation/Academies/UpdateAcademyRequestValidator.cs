using Academy.Application.Contracts.Academies;
using FluentValidation;

namespace Academy.Application.Validation.Academies;

public sealed class UpdateAcademyRequestValidator : AbstractValidator<UpdateAcademyRequest>
{
    public UpdateAcademyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);
    }
}
