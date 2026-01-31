using Academy.Application.Contracts.Sessions;
using FluentValidation;

namespace Academy.Application.Validation.Sessions;

public sealed class UpdateSessionRequestValidator : AbstractValidator<UpdateSessionRequest>
{
    public UpdateSessionRequestValidator()
    {
        RuleFor(x => x.InstructorUserId)
            .NotEmpty();

        RuleFor(x => x.StartsAtUtc)
            .NotEmpty();

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 360);

        RuleFor(x => x.Notes)
            .MaximumLength(800);
    }
}
