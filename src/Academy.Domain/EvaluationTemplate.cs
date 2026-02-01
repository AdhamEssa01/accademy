namespace Academy.Domain;

public sealed class EvaluationTemplate : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid? ProgramId { get; set; }

    public Guid? CourseId { get; set; }

    public Guid? LevelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
