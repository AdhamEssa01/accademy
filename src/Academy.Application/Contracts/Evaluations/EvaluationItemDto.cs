namespace Academy.Application.Contracts.Evaluations;

public sealed class EvaluationItemDto
{
    public Guid Id { get; set; }

    public Guid CriterionId { get; set; }

    public decimal Score { get; set; }

    public string? Comment { get; set; }
}
