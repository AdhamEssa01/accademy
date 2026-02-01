namespace Academy.Application.Contracts.Dashboards;

public sealed class InstructorDashboardDto
{
    public IReadOnlyList<InstructorSessionSummaryDto> TodaySessions { get; set; } = Array.Empty<InstructorSessionSummaryDto>();

    public int PendingManualGradingCount { get; set; }

    public IReadOnlyList<InstructorEvaluationSummaryDto> RecentEvaluations { get; set; } = Array.Empty<InstructorEvaluationSummaryDto>();
}
