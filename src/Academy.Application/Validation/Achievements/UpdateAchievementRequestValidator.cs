using Academy.Application.Contracts.Achievements;
using FluentValidation;

namespace Academy.Application.Validation.Achievements;

public sealed class UpdateAchievementRequestValidator : AbstractValidator<UpdateAchievementRequest>
{
    public UpdateAchievementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.MediaUrl)
            .MaximumLength(500);

        RuleFor(x => x.Tags)
            .MaximumLength(200);

        RuleFor(x => x.DateUtc)
            .NotEmpty();
    }
}
