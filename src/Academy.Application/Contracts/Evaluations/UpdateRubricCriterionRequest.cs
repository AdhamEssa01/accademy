namespace Academy.Application.Contracts.Evaluations;

public sealed class UpdateRubricCriterionRequest
{
    public string Name { get; set; } = string.Empty;

    public int MaxScore { get; set; }

    public decimal Weight { get; set; }

    public int SortOrder { get; set; }
}
