using Academy.Application.Contracts.Exams;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Exams;

public interface IExamAttemptService
{
    Task<ExamAttemptDto> StartAsync(Guid assignmentId, StartExamAttemptRequest request, CancellationToken ct);

    Task SaveAnswersAsync(Guid attemptId, SaveAttemptAnswersRequest request, CancellationToken ct);

    Task<ExamAttemptDto> SubmitAsync(Guid attemptId, CancellationToken ct);

    Task<PagedResponse<ExamResultDto>> ParentListMyChildrenAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);

    Task<PagedResponse<ExamResultDto>> ListForExamAsync(
        Guid examId,
        PagedRequest request,
        CancellationToken ct);
}
