namespace Academy.Application.Contracts.Evaluations;

public sealed class RubricCriterionDto
{
    public Guid Id { get; set; }

    public Guid TemplateId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int MaxScore { get; set; }

    public decimal Weight { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
