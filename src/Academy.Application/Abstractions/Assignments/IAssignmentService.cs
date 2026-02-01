using Academy.Application.Contracts.Assignments;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Assignments;

public interface IAssignmentService
{
    Task<AssignmentDto> CreateAsync(CreateAssignmentRequest request, CancellationToken ct);

    Task<PagedResponse<AssignmentDto>> ListForStaffAsync(
        Guid? groupId,
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);

    Task<PagedResponse<AssignmentDto>> ListForParentAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);
}
