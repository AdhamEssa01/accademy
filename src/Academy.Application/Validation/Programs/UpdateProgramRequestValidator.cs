using Academy.Application.Contracts.Programs;
using FluentValidation;

namespace Academy.Application.Validation.Programs;

public sealed class UpdateProgramRequestValidator : AbstractValidator<UpdateProgramRequest>
{
    public UpdateProgramRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(800);
    }
}
