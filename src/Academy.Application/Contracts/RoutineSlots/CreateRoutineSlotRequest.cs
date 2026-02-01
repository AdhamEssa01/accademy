namespace Academy.Application.Contracts.RoutineSlots;

public sealed record CreateRoutineSlotRequest(
    Guid GroupId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    Guid InstructorUserId);
