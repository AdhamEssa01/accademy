namespace Academy.Domain;

public sealed class RoutineSlot : IAcademyScoped
{
    public Guid Id { get; set; }
    public Guid AcademyId { get; set; }
    public Guid GroupId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public Guid InstructorUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
