using Academy.Application.Contracts.RoutineSlots;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Scheduling;

public interface IRoutineSlotService
{
    Task<PagedResponse<RoutineSlotDto>> ListAsync(PagedRequest request, CancellationToken ct);
    Task<RoutineSlotDto> GetAsync(Guid id, CancellationToken ct);
    Task<RoutineSlotDto> CreateAsync(CreateRoutineSlotRequest request, CancellationToken ct);
    Task<RoutineSlotDto> UpdateAsync(Guid id, UpdateRoutineSlotRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<PagedResponse<RoutineSlotDto>> ListMineAsync(Guid instructorUserId, PagedRequest request, CancellationToken ct);
    Task<int> GenerateSessionsAsync(DateOnly from, DateOnly to, CancellationToken ct);
}
