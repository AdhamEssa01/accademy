using Academy.Application.Abstractions.Academy;
using Academy.Application.Contracts.Academies;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/academies")]
public sealed class AcademiesController : ControllerBase
{
    private readonly IAcademyService _academyService;

    public AcademiesController(IAcademyService academyService)
    {
        _academyService = academyService;
    }

    [HttpGet("me")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<AcademyDto>> GetMyAcademy(CancellationToken ct)
    {
        var academy = await _academyService.GetMyAcademyAsync(ct);
        return Ok(academy);
    }

    [HttpPut("me")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<AcademyDto>> UpdateMyAcademy(
        [FromBody] UpdateAcademyRequest request,
        CancellationToken ct)
    {
        var academy = await _academyService.UpdateMyAcademyAsync(request, ct);
        return Ok(academy);
    }
}
