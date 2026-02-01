using Academy.Application.Contracts.Exams;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class AttemptAnswerRequestValidator : AbstractValidator<AttemptAnswerRequest>
{
    public AttemptAnswerRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x.AnswerJson)
            .NotEmpty();
    }
}
