using Academy.Application.Contracts.Evaluations;
using FluentValidation;

namespace Academy.Application.Validation.Evaluations;

public sealed class UpdateEvaluationTemplateRequestValidator : AbstractValidator<UpdateEvaluationTemplateRequest>
{
    public UpdateEvaluationTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(800)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
