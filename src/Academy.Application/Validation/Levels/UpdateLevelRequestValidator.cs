using Academy.Application.Contracts.Levels;
using FluentValidation;

namespace Academy.Application.Validation.Levels;

public sealed class UpdateLevelRequestValidator : AbstractValidator<UpdateLevelRequest>
{
    public UpdateLevelRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
