using Academy.Application.Contracts.Evaluations;
using FluentValidation;

namespace Academy.Application.Validation.Evaluations;

public sealed class CreateEvaluationRequestValidator : AbstractValidator<CreateEvaluationRequest>
{
    public CreateEvaluationRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.TemplateId)
            .NotEmpty();

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        RuleFor(x => x.Items)
            .NotNull()
            .Must(items => items is { Count: > 0 })
            .WithMessage("At least one item is required.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateEvaluationItemRequestValidator());
    }
}
