using Academy.Application.Contracts.Cms;
using FluentValidation;

namespace Academy.Application.Validation.Cms;

public sealed class UpdateCmsPageRequestValidator : AbstractValidator<UpdateCmsPageRequest>
{
    public UpdateCmsPageRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title is not null);
    }
}
