using Academy.Application.Abstractions.Behavior;
using Academy.Application.Contracts.Behavior;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/behavior-events")]
public sealed class BehaviorEventsController : ControllerBase
{
    private readonly IBehaviorService _behaviorService;

    public BehaviorEventsController(IBehaviorService behaviorService)
    {
        _behaviorService = behaviorService;
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<BehaviorEventDto>> Create(
        [FromBody] CreateBehaviorEventRequest request,
        CancellationToken ct)
    {
        var behaviorEvent = await _behaviorService.CreateAsync(request, ct);
        return Ok(behaviorEvent);
    }

    [HttpGet("/api/v{version:apiVersion}/students/{studentId:guid}/behavior-events")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<BehaviorEventDto>>> ListForStudent(
        Guid studentId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var eventsList = await _behaviorService.ListForStudentAsync(studentId, from, to, request, ct);
        return Ok(eventsList);
    }
}
