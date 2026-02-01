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
[Route("api/v{version:apiVersion}/parent/me/behavior-events")]
public sealed class ParentBehaviorEventsController : ControllerBase
{
    private readonly IBehaviorService _behaviorService;

    public ParentBehaviorEventsController(IBehaviorService behaviorService)
    {
        _behaviorService = behaviorService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<PagedResponse<BehaviorEventDto>>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var eventsList = await _behaviorService.ParentListMyChildrenAsync(from, to, request, ct);
        return Ok(eventsList);
    }
}
