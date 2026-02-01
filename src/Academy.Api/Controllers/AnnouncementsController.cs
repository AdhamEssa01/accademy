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
[Route("api/v{version:apiVersion}/announcements")]
public sealed class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<AnnouncementDto>> Create(
        [FromBody] CreateAnnouncementRequest request,
        CancellationToken ct)
    {
        var announcement = await _announcementService.CreateAsync(request, ct);
        return Ok(announcement);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<AnnouncementDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var announcements = await _announcementService.ListForStaffAsync(request, ct);
        return Ok(announcements);
    }
}
