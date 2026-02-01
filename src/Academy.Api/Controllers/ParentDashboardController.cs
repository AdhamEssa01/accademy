using Academy.Application.Abstractions.Dashboards;
using Academy.Application.Contracts.Dashboards;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboards/parent")]
public sealed class ParentDashboardController : ControllerBase
{
    private readonly IParentDashboardService _dashboardService;

    public ParentDashboardController(IParentDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<ParentDashboardDto>> Get(CancellationToken ct)
    {
        var dashboard = await _dashboardService.GetAsync(ct);
        return Ok(dashboard);
    }
}
