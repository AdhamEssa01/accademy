using Academy.Application.Contracts.Levels;
using FluentValidation;

namespace Academy.Application.Validation.Levels;

public sealed class CreateLevelRequestValidator : AbstractValidator<CreateLevelRequest>
{
    public CreateLevelRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
