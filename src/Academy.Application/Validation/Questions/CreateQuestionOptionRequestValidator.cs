using Academy.Application.Contracts.Questions;
using FluentValidation;

namespace Academy.Application.Validation.Questions;

public sealed class CreateQuestionOptionRequestValidator : AbstractValidator<CreateQuestionOptionRequest>
{
    public CreateQuestionOptionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
