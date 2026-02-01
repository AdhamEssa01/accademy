using Academy.Application.Contracts.Students;

namespace Academy.Application.Contracts.Dashboards;

public sealed class AdminDashboardDto
{
    public AttendanceSummaryDto AttendanceToday { get; set; } = new();

    public IReadOnlyList<StudentRiskDto> RiskyStudents { get; set; } = Array.Empty<StudentRiskDto>();

    public int PendingManualGradingCount { get; set; }

    public IReadOnlyList<ExamAttemptDailyCountDto> ExamAttemptsLast7Days { get; set; } = Array.Empty<ExamAttemptDailyCountDto>();
}
