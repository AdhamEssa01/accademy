namespace Academy.Domain;

public sealed class RubricCriterion : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid TemplateId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int MaxScore { get; set; }

    public decimal Weight { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
