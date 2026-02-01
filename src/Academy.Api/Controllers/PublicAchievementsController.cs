using Academy.Application.Abstractions.Achievements;
using Academy.Application.Contracts.Achievements;
using Academy.Shared.Pagination;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/public/achievements")]
public sealed class PublicAchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public PublicAchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<AchievementDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _achievementService.ListPublicAsync(request, ct);
        return Ok(response);
    }
}
