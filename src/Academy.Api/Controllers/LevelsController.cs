using Academy.Application.Abstractions.Catalog;
using Academy.Application.Contracts.Levels;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/levels")]
public sealed class LevelsController : ControllerBase
{
    private readonly IProgramCatalogService _catalogService;

    public LevelsController(IProgramCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<LevelDto>>> List(
        [FromQuery] Guid? courseId,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var levels = await _catalogService.ListLevelsAsync(courseId, request, ct);
        return Ok(levels);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<LevelDto>> Get(Guid id, CancellationToken ct)
    {
        var level = await _catalogService.GetLevelAsync(id, ct);
        return Ok(level);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<LevelDto>> Create(
        [FromBody] CreateLevelRequest request,
        CancellationToken ct)
    {
        var level = await _catalogService.CreateLevelAsync(request, ct);
        return Ok(level);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<LevelDto>> Update(
        Guid id,
        [FromBody] UpdateLevelRequest request,
        CancellationToken ct)
    {
        var level = await _catalogService.UpdateLevelAsync(id, request, ct);
        return Ok(level);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _catalogService.DeleteLevelAsync(id, ct);
        return NoContent();
    }
}
