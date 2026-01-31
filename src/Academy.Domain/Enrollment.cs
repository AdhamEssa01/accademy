namespace Academy.Domain;

public sealed class Enrollment : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid StudentId { get; set; }

    public Guid GroupId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
