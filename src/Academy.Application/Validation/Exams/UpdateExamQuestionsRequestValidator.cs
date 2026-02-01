using Academy.Application.Contracts.Exams;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class UpdateExamQuestionsRequestValidator : AbstractValidator<UpdateExamQuestionsRequest>
{
    public UpdateExamQuestionsRequestValidator()
    {
        RuleFor(x => x.Questions)
            .NotNull();

        RuleForEach(x => x.Questions)
            .SetValidator(new ExamQuestionItemRequestValidator());
    }
}
