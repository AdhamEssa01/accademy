using Academy.Application.Contracts.Questions;
using Academy.Domain;
using FluentValidation;

namespace Academy.Application.Validation.Questions;

public sealed class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Difficulty)
            .IsInEnum();

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Tags)
            .MaximumLength(500);

        RuleFor(x => x.Options)
            .NotNull();

        RuleFor(x => x.Options)
            .Must(options => options is { Count: >= 2 })
            .When(x => x.Type == QuestionType.MCQ || x.Type == QuestionType.TrueFalse)
            .WithMessage("Options are required for MCQ/TrueFalse questions.");

        RuleForEach(x => x.Options)
            .SetValidator(new CreateQuestionOptionRequestValidator());
    }
}
