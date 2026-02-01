using Academy.Application.Contracts.Notifications;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Notifications;

public interface INotificationService
{
    Task<PagedResponse<NotificationDto>> ListForCurrentUserAsync(PagedRequest request, CancellationToken ct);

    Task MarkReadAsync(Guid id, CancellationToken ct);
}
