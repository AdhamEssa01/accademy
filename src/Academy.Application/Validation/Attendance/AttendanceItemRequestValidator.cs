using Academy.Application.Contracts.Attendance;
using FluentValidation;

namespace Academy.Application.Validation.Attendance;

public sealed class AttendanceItemRequestValidator : AbstractValidator<AttendanceItemRequest>
{
    public AttendanceItemRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Reason)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason));

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
