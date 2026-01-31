using Academy.Application.Contracts.Enrollments;
using FluentValidation;

namespace Academy.Application.Validation.Enrollments;

public sealed class EndEnrollmentRequestValidator : AbstractValidator<EndEnrollmentRequest>
{
    public EndEnrollmentRequestValidator()
    {
        RuleFor(x => x.EndDate)
            .NotEmpty();
    }
}
