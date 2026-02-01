using Academy.Application.Abstractions.Dashboards;
using Academy.Application.Contracts.Dashboards;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboards/admin")]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<AdminDashboardDto>> Get(CancellationToken ct)
    {
        var dashboard = await _adminDashboardService.GetAsync(ct);
        return Ok(dashboard);
    }
}
