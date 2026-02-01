namespace Academy.Application.Contracts.RoutineSlots;

public sealed record UpdateRoutineSlotRequest(
    Guid GroupId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    int DurationMinutes,
    Guid InstructorUserId);
