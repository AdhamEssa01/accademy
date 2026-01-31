using Academy.Application.Abstractions.Academy;
using Academy.Application.Contracts.Branches;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/branches")]
public sealed class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<BranchDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var branches = await _branchService.ListAsync(request, ct);
        return Ok(branches);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<BranchDto>> Get(Guid id, CancellationToken ct)
    {
        var branch = await _branchService.GetAsync(id, ct);
        return Ok(branch);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<BranchDto>> Create(
        [FromBody] CreateBranchRequest request,
        CancellationToken ct)
    {
        var branch = await _branchService.CreateAsync(request, ct);
        return Ok(branch);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<BranchDto>> Update(
        Guid id,
        [FromBody] UpdateBranchRequest request,
        CancellationToken ct)
    {
        var branch = await _branchService.UpdateAsync(id, request, ct);
        return Ok(branch);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _branchService.DeleteAsync(id, ct);
        return NoContent();
    }
}
