using Academy.Application.Contracts.Groups;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Scheduling;

public interface IGroupService
{
    Task<PagedResponse<GroupDto>> ListAsync(PagedRequest request, CancellationToken ct);

    Task<GroupDto> GetAsync(Guid id, CancellationToken ct);

    Task<GroupDto> CreateAsync(CreateGroupRequest request, CancellationToken ct);

    Task<GroupDto> UpdateAsync(Guid id, UpdateGroupRequest request, CancellationToken ct);

    Task<GroupDto> AssignInstructorAsync(Guid id, AssignInstructorRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<GroupDto>> ListMineAsync(Guid instructorUserId, PagedRequest request, CancellationToken ct);
}
