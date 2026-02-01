using Academy.Application.Contracts.Evaluations;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Evaluations;

public interface IEvaluationService
{
    Task<EvaluationDto> CreateAsync(CreateEvaluationRequest request, CancellationToken ct);

    Task<PagedResponse<EvaluationDto>> ListForStudentAsync(Guid studentId, PagedRequest request, CancellationToken ct);

    Task<PagedResponse<EvaluationDto>> ParentListMyChildrenAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);
}
