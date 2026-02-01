namespace Academy.Application.Contracts.RoutineSlots;

public sealed record RoutineSlotDto
{
    public Guid Id { get; init; }
    public Guid AcademyId { get; init; }
    public Guid GroupId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public int DurationMinutes { get; init; }
    public Guid InstructorUserId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
