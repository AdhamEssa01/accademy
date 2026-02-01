using Academy.Application.Contracts.Exams;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class GradeAttemptAnswerRequestValidator : AbstractValidator<GradeAttemptAnswerRequest>
{
    public GradeAttemptAnswerRequestValidator()
    {
        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Feedback)
            .MaximumLength(500);
    }
}
