using Academy.Application.Contracts.Evaluations;
using FluentValidation;

namespace Academy.Application.Validation.Evaluations;

public sealed class CreateEvaluationItemRequestValidator : AbstractValidator<CreateEvaluationItemRequest>
{
    public CreateEvaluationItemRequestValidator()
    {
        RuleFor(x => x.CriterionId)
            .NotEmpty();

        RuleFor(x => x.Score)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.Comment)
            .MaximumLength(500);
    }
}
