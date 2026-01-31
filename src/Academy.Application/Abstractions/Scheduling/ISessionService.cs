using Academy.Application.Contracts.Sessions;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Scheduling;

public interface ISessionService
{
    Task<PagedResponse<SessionDto>> ListAsync(
        Guid? groupId,
        DateTime? fromUtc,
        DateTime? toUtc,
        PagedRequest request,
        CancellationToken ct);

    Task<SessionDto> GetAsync(Guid id, CancellationToken ct);

    Task<SessionDto> CreateAsync(CreateSessionRequest request, CancellationToken ct);

    Task<SessionDto> UpdateAsync(Guid id, UpdateSessionRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<SessionDto>> ListMineAsync(
        Guid instructorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        PagedRequest request,
        CancellationToken ct);
}
