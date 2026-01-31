using Academy.Application.Contracts.Courses;
using FluentValidation;

namespace Academy.Application.Validation.Courses;

public sealed class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.ProgramId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(800);
    }
}
