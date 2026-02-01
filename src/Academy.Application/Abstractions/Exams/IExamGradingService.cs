namespace Academy.Application.Abstractions.Exams;

public interface IExamGradingService
{
    Task GradeAttemptAsync(Guid attemptId, CancellationToken ct);
}
