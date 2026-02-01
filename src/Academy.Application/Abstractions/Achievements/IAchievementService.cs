using Academy.Application.Contracts.Achievements;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Achievements;

public interface IAchievementService
{
    Task<PagedResponse<AchievementDto>> ListAsync(PagedRequest request, CancellationToken ct);

    Task<AchievementDto> GetAsync(Guid id, CancellationToken ct);

    Task<AchievementDto> CreateAsync(CreateAchievementRequest request, CancellationToken ct);

    Task<AchievementDto> UpdateAsync(Guid id, UpdateAchievementRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<AchievementDto>> ListPublicAsync(PagedRequest request, CancellationToken ct);
}
