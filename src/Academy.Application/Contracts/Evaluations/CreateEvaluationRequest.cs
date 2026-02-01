namespace Academy.Application.Contracts.Evaluations;

public sealed class CreateEvaluationRequest
{
    public Guid StudentId { get; set; }

    public Guid TemplateId { get; set; }

    public Guid? SessionId { get; set; }

    public string? Notes { get; set; }

    public List<CreateEvaluationItemRequest> Items { get; set; } = new();
}
