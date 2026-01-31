using Academy.Application.Contracts.Enrollments;
using FluentValidation;

namespace Academy.Application.Validation.Enrollments;

public sealed class CreateEnrollmentRequestValidator : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.GroupId)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .NotEmpty();
    }
}
