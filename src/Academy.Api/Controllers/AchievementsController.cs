using Academy.Application.Abstractions.Achievements;
using Academy.Application.Contracts.Achievements;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/achievements")]
public sealed class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<AchievementDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _achievementService.ListAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<AchievementDto>> Get(Guid id, CancellationToken ct)
    {
        var achievement = await _achievementService.GetAsync(id, ct);
        return Ok(achievement);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<AchievementDto>> Create(
        [FromBody] CreateAchievementRequest request,
        CancellationToken ct)
    {
        var achievement = await _achievementService.CreateAsync(request, ct);
        return Ok(achievement);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<AchievementDto>> Update(
        Guid id,
        [FromBody] UpdateAchievementRequest request,
        CancellationToken ct)
    {
        var achievement = await _achievementService.UpdateAsync(id, request, ct);
        return Ok(achievement);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _achievementService.DeleteAsync(id, ct);
        return NoContent();
    }
}
