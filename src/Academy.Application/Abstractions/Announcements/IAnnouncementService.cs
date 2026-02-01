using Academy.Application.Contracts.Announcements;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Announcements;

public interface IAnnouncementService
{
    Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequest request, CancellationToken ct);

    Task<PagedResponse<AnnouncementDto>> ListForStaffAsync(PagedRequest request, CancellationToken ct);

    Task<PagedResponse<AnnouncementDto>> ListForParentAsync(PagedRequest request, CancellationToken ct);
}
