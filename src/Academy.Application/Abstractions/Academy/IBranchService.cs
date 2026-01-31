using Academy.Application.Contracts.Branches;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Academy;

public interface IBranchService
{
    Task<PagedResponse<BranchDto>> ListAsync(PagedRequest request, CancellationToken ct);

    Task<BranchDto> GetAsync(Guid id, CancellationToken ct);

    Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken ct);

    Task<BranchDto> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);
}
