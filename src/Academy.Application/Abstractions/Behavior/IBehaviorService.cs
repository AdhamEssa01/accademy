using Academy.Application.Contracts.Behavior;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Behavior;

public interface IBehaviorService
{
    Task<BehaviorEventDto> CreateAsync(CreateBehaviorEventRequest request, CancellationToken ct);

    Task<PagedResponse<BehaviorEventDto>> ListForStudentAsync(
        Guid studentId,
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);

    Task<PagedResponse<BehaviorEventDto>> ParentListMyChildrenAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);
}
