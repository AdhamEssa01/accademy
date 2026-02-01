using Academy.Application.Contracts.Cms;
using FluentValidation;

namespace Academy.Application.Validation.Cms;

public sealed class CmsSectionUpsertRequestValidator : AbstractValidator<CmsSectionUpsertRequest>
{
    public CmsSectionUpsertRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.JsonContent)
            .NotEmpty();

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
