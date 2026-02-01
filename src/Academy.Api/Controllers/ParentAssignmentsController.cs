using Academy.Application.Abstractions.Assignments;
using Academy.Application.Contracts.Assignments;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parent/me/assignments")]
public sealed class ParentAssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public ParentAssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<PagedResponse<AssignmentDto>>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var assignments = await _assignmentService.ListForParentAsync(from, to, request, ct);
        return Ok(assignments);
    }
}
