using Academy.Application.Contracts.Exams;

namespace Academy.Application.Abstractions.Exams;

public interface IExamAssignmentService
{
    Task<ExamAssignmentDto> CreateAsync(Guid examId, CreateExamAssignmentRequest request, CancellationToken ct);

    Task<IReadOnlyList<ExamAssignmentDto>> ListAsync(Guid examId, CancellationToken ct);
}
