using Academy.Application.Contracts.Guardians;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Guardians;

public interface IGuardianService
{
    Task<PagedResponse<GuardianDto>> ListAsync(PagedRequest request, CancellationToken ct);

    Task<GuardianDto> GetAsync(Guid id, CancellationToken ct);

    Task<GuardianDto> CreateAsync(CreateGuardianRequest request, CancellationToken ct);

    Task<GuardianDto> UpdateAsync(Guid id, UpdateGuardianRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);

    Task LinkGuardianToStudentAsync(Guid guardianId, Guid studentId, LinkGuardianToStudentRequest request, CancellationToken ct);

    Task LinkGuardianToUserAsync(Guid guardianId, LinkGuardianToUserRequest request, CancellationToken ct);
}
