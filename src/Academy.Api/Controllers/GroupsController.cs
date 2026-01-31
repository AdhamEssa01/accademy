using Academy.Application.Abstractions.Scheduling;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Groups;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/groups")]
public sealed class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ICurrentUserContext _currentUserContext;

    public GroupsController(IGroupService groupService, ICurrentUserContext currentUserContext)
    {
        _groupService = groupService;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<GroupDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var groups = await _groupService.ListAsync(request, ct);
        return Ok(groups);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<GroupDto>> Get(Guid id, CancellationToken ct)
    {
        var group = await _groupService.GetAsync(id, ct);
        return Ok(group);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<GroupDto>> Create(
        [FromBody] CreateGroupRequest request,
        CancellationToken ct)
    {
        var group = await _groupService.CreateAsync(request, ct);
        return Ok(group);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<GroupDto>> Update(
        Guid id,
        [FromBody] UpdateGroupRequest request,
        CancellationToken ct)
    {
        var group = await _groupService.UpdateAsync(id, request, ct);
        return Ok(group);
    }

    [HttpPost("{id:guid}/assign-instructor")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<GroupDto>> AssignInstructor(
        Guid id,
        [FromBody] AssignInstructorRequest request,
        CancellationToken ct)
    {
        var group = await _groupService.AssignInstructorAsync(id, request, ct);
        return Ok(group);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _groupService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("mine")]
    [Authorize(Policy = Policies.Instructor)]
    public async Task<ActionResult<PagedResponse<GroupDto>>> ListMine(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return Unauthorized();
        }

        var groups = await _groupService.ListMineAsync(_currentUserContext.UserId.Value, request, ct);
        return Ok(groups);
    }
}
