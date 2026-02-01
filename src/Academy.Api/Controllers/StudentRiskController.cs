using Academy.Application.Abstractions.Students;
using Academy.Application.Contracts.Students;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/students/risk")]
public sealed class StudentRiskController : ControllerBase
{
    private readonly IStudentRiskService _studentRiskService;

    public StudentRiskController(IStudentRiskService studentRiskService)
    {
        _studentRiskService = studentRiskService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<StudentRiskDto>>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _studentRiskService.GetRiskListAsync(from, to, request, ct);
        return Ok(response);
    }
}
