using Academy.Application.Contracts.Exams;

namespace Academy.Application.Abstractions.Exams;

public interface IExamManualGradingService
{
    Task GradeAnswerAsync(Guid answerId, GradeAttemptAnswerRequest request, CancellationToken ct);
}
