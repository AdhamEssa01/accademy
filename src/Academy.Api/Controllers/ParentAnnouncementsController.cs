using Academy.Application.Abstractions.Announcements;
using Academy.Application.Contracts.Announcements;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parent/me/announcements")]
public sealed class ParentAnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public ParentAnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<PagedResponse<AnnouncementDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var announcements = await _announcementService.ListForParentAsync(request, ct);
        return Ok(announcements);
    }
}
