using Academy.Application.Abstractions.Attendance;
using Academy.Application.Contracts.Attendance;
using Academy.Domain;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/attendance")]
public sealed class AttendanceController : ControllerBase
{
    private readonly IAttendanceQueryService _attendanceQueryService;

    public AttendanceController(IAttendanceQueryService attendanceQueryService)
    {
        _attendanceQueryService = attendanceQueryService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<AttendanceRecordDto>>> List(
        [FromQuery] Guid? groupId,
        [FromQuery] Guid? studentId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] AttendanceStatus? status,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _attendanceQueryService.ListAsync(groupId, studentId, from, to, status, request, ct);
        return Ok(response);
    }
}
