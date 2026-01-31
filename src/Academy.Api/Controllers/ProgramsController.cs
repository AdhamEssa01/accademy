using Academy.Application.Abstractions.Catalog;
using Academy.Application.Contracts.Programs;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/programs")]
public sealed class ProgramsController : ControllerBase
{
    private readonly IProgramCatalogService _catalogService;

    public ProgramsController(IProgramCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<ProgramDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var programs = await _catalogService.ListProgramsAsync(request, ct);
        return Ok(programs);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<ProgramDto>> Get(Guid id, CancellationToken ct)
    {
        var program = await _catalogService.GetProgramAsync(id, ct);
        return Ok(program);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<ProgramDto>> Create(
        [FromBody] CreateProgramRequest request,
        CancellationToken ct)
    {
        var program = await _catalogService.CreateProgramAsync(request, ct);
        return Ok(program);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<ProgramDto>> Update(
        Guid id,
        [FromBody] UpdateProgramRequest request,
        CancellationToken ct)
    {
        var program = await _catalogService.UpdateProgramAsync(id, request, ct);
        return Ok(program);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _catalogService.DeleteProgramAsync(id, ct);
        return NoContent();
    }
}
