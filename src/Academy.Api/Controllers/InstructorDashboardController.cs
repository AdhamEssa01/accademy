using Academy.Application.Abstractions.Dashboards;
using Academy.Application.Contracts.Dashboards;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboards/instructor")]
public sealed class InstructorDashboardController : ControllerBase
{
    private readonly IInstructorDashboardService _dashboardService;

    public InstructorDashboardController(IInstructorDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Instructor)]
    public async Task<ActionResult<InstructorDashboardDto>> Get(CancellationToken ct)
    {
        var dashboard = await _dashboardService.GetAsync(ct);
        return Ok(dashboard);
    }
}
