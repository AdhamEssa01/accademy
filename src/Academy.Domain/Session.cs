namespace Academy.Domain;

public sealed class Session : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid GroupId { get; set; }

    public Guid InstructorUserId { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public int DurationMinutes { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
