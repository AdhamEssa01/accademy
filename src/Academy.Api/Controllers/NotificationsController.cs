using Academy.Application.Abstractions.Notifications;
using Academy.Application.Contracts.Notifications;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.AnyAuthenticated)]
    public async Task<ActionResult<PagedResponse<NotificationDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var notifications = await _notificationService.ListForCurrentUserAsync(request, ct);
        return Ok(notifications);
    }

    [HttpPost("{id:guid}/read")]
    [Authorize(Policy = Policies.AnyAuthenticated)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await _notificationService.MarkReadAsync(id, ct);
        return NoContent();
    }
}
