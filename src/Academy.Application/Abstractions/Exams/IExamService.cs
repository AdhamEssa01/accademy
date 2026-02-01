using Academy.Application.Contracts.Exams;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Exams;

public interface IExamService
{
    Task<PagedResponse<ExamDto>> ListAsync(PagedRequest request, CancellationToken ct);

    Task<ExamDto> GetAsync(Guid id, CancellationToken ct);

    Task<ExamDto> CreateAsync(CreateExamRequest request, CancellationToken ct);

    Task<ExamDto> UpdateAsync(Guid id, UpdateExamRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<ExamQuestionDto>> UpdateQuestionsAsync(
        Guid examId,
        UpdateExamQuestionsRequest request,
        CancellationToken ct);
}
