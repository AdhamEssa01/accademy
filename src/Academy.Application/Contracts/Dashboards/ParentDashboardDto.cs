using Academy.Application.Contracts.Assignments;
using Academy.Application.Contracts.Exams;

namespace Academy.Application.Contracts.Dashboards;

public sealed class ParentDashboardDto
{
    public IReadOnlyList<ParentChildSummaryDto> Children { get; set; } = Array.Empty<ParentChildSummaryDto>();

    public IReadOnlyList<AssignmentDto> UpcomingAssignments { get; set; } = Array.Empty<AssignmentDto>();

    public int NewAnnouncementsCount { get; set; }

    public IReadOnlyList<ExamResultDto> RecentExamResults { get; set; } = Array.Empty<ExamResultDto>();
}
