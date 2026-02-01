namespace Academy.Application.Contracts.Dashboards;

public sealed class ParentChildSummaryDto
{
    public Guid StudentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public decimal AttendanceRate { get; set; }

    public decimal LastEvaluationScore { get; set; }

    public int BehaviorPoints { get; set; }
}
