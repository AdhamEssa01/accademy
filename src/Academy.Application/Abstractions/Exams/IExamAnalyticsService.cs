using Academy.Application.Contracts.Exams;

namespace Academy.Application.Abstractions.Exams;

public interface IExamAnalyticsService
{
    Task<ExamStatsDto> GetStatsAsync(Guid examId, CancellationToken ct);
}
