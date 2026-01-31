namespace Academy.Domain;

public sealed class StudentGuardian : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid StudentId { get; set; }

    public Guid GuardianId { get; set; }

    public string? Relation { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
