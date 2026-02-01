namespace Academy.Domain;

public sealed class AssignmentTarget : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid AssignmentId { get; set; }

    public Guid? StudentId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
