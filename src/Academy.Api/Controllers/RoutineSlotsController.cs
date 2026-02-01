using Academy.Application.Abstractions.Scheduling;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.RoutineSlots;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/routine-slots")]
public sealed class RoutineSlotsController : ControllerBase
{
    private readonly IRoutineSlotService _routineSlotService;
    private readonly ICurrentUserContext _currentUserContext;

    public RoutineSlotsController(IRoutineSlotService routineSlotService, ICurrentUserContext currentUserContext)
    {
        _routineSlotService = routineSlotService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<RoutineSlotDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var slots = await _routineSlotService.ListAsync(request, ct);
        return Ok(slots);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<RoutineSlotDto>> Get(Guid id, CancellationToken ct)
    {
        var slot = await _routineSlotService.GetAsync(id, ct);
        return Ok(slot);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<RoutineSlotDto>> Create(
        [FromBody] CreateRoutineSlotRequest request,
        CancellationToken ct)
    {
        var slot = await _routineSlotService.CreateAsync(request, ct);
        return Ok(slot);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<RoutineSlotDto>> Update(
        Guid id,
        [FromBody] UpdateRoutineSlotRequest request,
        CancellationToken ct)
    {
        var slot = await _routineSlotService.UpdateAsync(id, request, ct);
        return Ok(slot);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _routineSlotService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("mine")]
    [Authorize(Policy = Policies.Instructor)]
    public async Task<ActionResult<PagedResponse<RoutineSlotDto>>> ListMine(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var slots = await _routineSlotService.ListMineAsync(_currentUserContext.UserId.Value, request, ct);
        return Ok(slots);
    }

    [HttpPost("generate-sessions")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<object>> GenerateSessions(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (from == default || to == default)
        {
            return BadRequest();
        }

        var created = await _routineSlotService.GenerateSessionsAsync(from, to, ct);
        return Ok(new { created });
    }
}
