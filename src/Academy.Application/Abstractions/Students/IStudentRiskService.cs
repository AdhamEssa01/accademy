using Academy.Application.Contracts.Students;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Students;

public interface IStudentRiskService
{
    Task<PagedResponse<StudentRiskDto>> GetRiskListAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);
}
