using Academy.Application.Contracts.Cms;
using FluentValidation;

namespace Academy.Application.Validation.Cms;

public sealed class UpdateCmsSectionsRequestValidator : AbstractValidator<UpdateCmsSectionsRequest>
{
    public UpdateCmsSectionsRequestValidator()
    {
        RuleFor(x => x.Sections)
            .NotNull();

        RuleForEach(x => x.Sections)
            .SetValidator(new CmsSectionUpsertRequestValidator());
    }
}
