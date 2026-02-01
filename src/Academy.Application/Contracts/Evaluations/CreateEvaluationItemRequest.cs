namespace Academy.Application.Contracts.Evaluations;

public sealed class CreateEvaluationItemRequest
{
    public Guid CriterionId { get; set; }

    public decimal Score { get; set; }

    public string? Comment { get; set; }
}
