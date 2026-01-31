using Academy.Application.Contracts.Courses;
using FluentValidation;

namespace Academy.Application.Validation.Courses;

public sealed class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(800);
    }
}
