using Academy.Application.Abstractions.Guardians;
using Academy.Application.Contracts.Guardians;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/guardians")]
public sealed class GuardiansController : ControllerBase
{
    private readonly IGuardianService _guardianService;

    public GuardiansController(IGuardianService guardianService)
    {
        _guardianService = guardianService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<GuardianDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var guardians = await _guardianService.ListAsync(request, ct);
        return Ok(guardians);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<GuardianDto>> Get(Guid id, CancellationToken ct)
    {
        var guardian = await _guardianService.GetAsync(id, ct);
        return Ok(guardian);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<GuardianDto>> Create(
        [FromBody] CreateGuardianRequest request,
        CancellationToken ct)
    {
        var guardian = await _guardianService.CreateAsync(request, ct);
        return Ok(guardian);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<GuardianDto>> Update(
        Guid id,
        [FromBody] UpdateGuardianRequest request,
        CancellationToken ct)
    {
        var guardian = await _guardianService.UpdateAsync(id, request, ct);
        return Ok(guardian);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _guardianService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{guardianId:guid}/link-user")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> LinkUser(
        Guid guardianId,
        [FromBody] LinkGuardianToUserRequest request,
        CancellationToken ct)
    {
        await _guardianService.LinkGuardianToUserAsync(guardianId, request, ct);
        return NoContent();
    }

    [HttpPost("/api/v{version:apiVersion}/students/{studentId:guid}/guardians/{guardianId:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> LinkStudent(
        Guid studentId,
        Guid guardianId,
        [FromBody] LinkGuardianToStudentRequest request,
        CancellationToken ct)
    {
        await _guardianService.LinkGuardianToStudentAsync(guardianId, studentId, request, ct);
        return NoContent();
    }
}
