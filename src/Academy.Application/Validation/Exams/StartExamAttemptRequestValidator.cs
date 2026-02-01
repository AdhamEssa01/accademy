using Academy.Application.Contracts.Exams;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class StartExamAttemptRequestValidator : AbstractValidator<StartExamAttemptRequest>
{
    public StartExamAttemptRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty();
    }
}
