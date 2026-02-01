using Academy.Application.Contracts.Exams;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class SaveAttemptAnswersRequestValidator : AbstractValidator<SaveAttemptAnswersRequest>
{
    public SaveAttemptAnswersRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotNull()
            .Must(list => list is { Count: > 0 })
            .WithMessage("At least one answer is required.");

        RuleForEach(x => x.Answers)
            .SetValidator(new AttemptAnswerRequestValidator());
    }
}
