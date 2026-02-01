using Academy.Application.Contracts.Announcements;
using Academy.Domain;
using FluentValidation;

namespace Academy.Application.Validation.Announcements;

public sealed class CreateAnnouncementRequestValidator : AbstractValidator<CreateAnnouncementRequest>
{
    public CreateAnnouncementRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(5000);

        RuleFor(x => x.Audience)
            .IsInEnum();

        RuleFor(x => x.GroupId)
            .NotEmpty()
            .When(x => x.Audience == AnnouncementAudience.GroupParents || x.Audience == AnnouncementAudience.GroupStaff);

        RuleFor(x => x.GroupId)
            .Empty()
            .When(x => x.Audience == AnnouncementAudience.AllParents || x.Audience == AnnouncementAudience.AllStaff);
    }
}
