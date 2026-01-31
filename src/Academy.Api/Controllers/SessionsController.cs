using Academy.Application.Abstractions.Scheduling;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Sessions;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ICurrentUserContext _currentUserContext;

    public SessionsController(ISessionService sessionService, ICurrentUserContext currentUserContext)
    {
        _sessionService = sessionService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<SessionDto>>> List(
        [FromQuery] Guid? groupId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var sessions = await _sessionService.ListAsync(groupId, fromUtc, toUtc, request, ct);
        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<SessionDto>> Get(Guid id, CancellationToken ct)
    {
        var session = await _sessionService.GetAsync(id, ct);
        return Ok(session);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<SessionDto>> Create(
        [FromBody] CreateSessionRequest request,
        CancellationToken ct)
    {
        var session = await _sessionService.CreateAsync(request, ct);
        return Ok(session);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<SessionDto>> Update(
        Guid id,
        [FromBody] UpdateSessionRequest request,
        CancellationToken ct)
    {
        var session = await _sessionService.UpdateAsync(id, request, ct);
        return Ok(session);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sessionService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("mine")]
    [Authorize(Policy = Policies.Instructor)]
    public async Task<ActionResult<PagedResponse<SessionDto>>> ListMine(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var sessions = await _sessionService.ListMineAsync(_currentUserContext.UserId.Value, fromUtc, toUtc, request, ct);
        return Ok(sessions);
    }
}
