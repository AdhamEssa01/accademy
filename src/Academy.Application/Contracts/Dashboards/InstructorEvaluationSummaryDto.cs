namespace Academy.Application.Contracts.Dashboards;

public sealed class InstructorEvaluationSummaryDto
{
    public Guid EvaluationId { get; set; }

    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public decimal TotalScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
