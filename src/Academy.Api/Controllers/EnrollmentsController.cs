using Academy.Application.Abstractions.Enrollments;
using Academy.Application.Contracts.Enrollments;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enrollments")]
public sealed class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<EnrollmentDto>> Create(
        [FromBody] CreateEnrollmentRequest request,
        CancellationToken ct)
    {
        var enrollment = await _enrollmentService.EnrollAsync(request, ct);
        return Ok(enrollment);
    }

    [HttpPost("{id:guid}/end")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<EnrollmentDto>> End(
        Guid id,
        [FromBody] EndEnrollmentRequest request,
        CancellationToken ct)
    {
        var enrollment = await _enrollmentService.EndAsync(id, request, ct);
        return Ok(enrollment);
    }

    [HttpGet("/api/v{version:apiVersion}/students/{studentId:guid}/enrollments")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> ListByStudent(
        Guid studentId,
        CancellationToken ct)
    {
        var enrollments = await _enrollmentService.ListByStudentAsync(studentId, ct);
        return Ok(enrollments);
    }

    [HttpGet("/api/v{version:apiVersion}/groups/{groupId:guid}/enrollments")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> ListByGroup(
        Guid groupId,
        CancellationToken ct)
    {
        var enrollments = await _enrollmentService.ListByGroupAsync(groupId, ct);
        return Ok(enrollments);
    }
}
