using Academy.Application.Contracts.Attendance;
using FluentValidation;

namespace Academy.Application.Validation.Attendance;

public sealed class SubmitAttendanceRequestValidator : AbstractValidator<SubmitAttendanceRequest>
{
    public SubmitAttendanceRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .Must(items => items.Count > 0)
            .WithMessage("At least one attendance item is required.");

        RuleForEach(x => x.Items)
            .SetValidator(new AttendanceItemRequestValidator());
    }
}
