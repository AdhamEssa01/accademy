using Academy.Application.Contracts.Evaluations;
using FluentValidation;

namespace Academy.Application.Validation.Evaluations;

public sealed class CreateRubricCriterionRequestValidator : AbstractValidator<CreateRubricCriterionRequest>
{
    public CreateRubricCriterionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.MaxScore)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Weight)
            .InclusiveBetween(0m, 10m);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
