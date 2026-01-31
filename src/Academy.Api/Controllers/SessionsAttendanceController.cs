using Academy.Application.Abstractions.Attendance;
using Academy.Application.Contracts.Attendance;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sessions/{sessionId:guid}/attendance")]
public sealed class SessionsAttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public SessionsAttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> Submit(
        Guid sessionId,
        [FromBody] SubmitAttendanceRequest request,
        CancellationToken ct)
    {
        var records = await _attendanceService.SubmitForSessionAsync(sessionId, request, ct);
        return Ok(records);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IReadOnlyList<AttendanceRecordDto>>> List(
        Guid sessionId,
        CancellationToken ct)
    {
        var records = await _attendanceService.ListForSessionAsync(sessionId, ct);
        return Ok(records);
    }
}
