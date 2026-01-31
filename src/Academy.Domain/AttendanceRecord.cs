namespace Academy.Domain;

public sealed class AttendanceRecord : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid SessionId { get; set; }

    public Guid StudentId { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Reason { get; set; }

    public string? Note { get; set; }

    public Guid MarkedByUserId { get; set; }

    public DateTime MarkedAtUtc { get; set; }
}
