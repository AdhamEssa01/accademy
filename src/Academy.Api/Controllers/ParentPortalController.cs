using Academy.Application.Abstractions.Parents;
using Academy.Application.Contracts.Students;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parent/me")]
public sealed class ParentPortalController : ControllerBase
{
    private readonly IParentPortalService _parentPortalService;

    public ParentPortalController(IParentPortalService parentPortalService)
    {
        _parentPortalService = parentPortalService;
    }

    [HttpGet("children")]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetChildren(CancellationToken ct)
    {
        var students = await _parentPortalService.GetMyChildrenAsync(ct);
        return Ok(students);
    }
}
