using Academy.Application.Contracts.RoutineSlots;
using FluentValidation;

namespace Academy.Application.Validation.RoutineSlots;

public sealed class UpdateRoutineSlotRequestValidator : AbstractValidator<UpdateRoutineSlotRequest>
{
    public UpdateRoutineSlotRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty();

        RuleFor(x => (int)x.DayOfWeek)
            .InclusiveBetween(0, 6);

        RuleFor(x => x.StartTime)
            .NotEqual(default(TimeOnly));

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 360);

        RuleFor(x => x.InstructorUserId)
            .NotEmpty();
    }
}
