using Academy.Application.Contracts.Exams;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class ExamQuestionItemRequestValidator : AbstractValidator<ExamQuestionItemRequest>
{
    public ExamQuestionItemRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty();

        RuleFor(x => x.Points)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
