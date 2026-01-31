using Academy.Application.Abstractions.Attendance;
using Academy.Application.Contracts.Attendance;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parent/me/attendance")]
public sealed class ParentAttendanceController : ControllerBase
{
    private readonly IAttendanceQueryService _attendanceQueryService;

    public ParentAttendanceController(IAttendanceQueryService attendanceQueryService)
    {
        _attendanceQueryService = attendanceQueryService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<PagedResponse<AttendanceRecordDto>>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _attendanceQueryService.ParentListForMyChildrenAsync(from, to, request, ct);
        return Ok(response);
    }
}
