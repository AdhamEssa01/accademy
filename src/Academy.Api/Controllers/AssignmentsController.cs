using Academy.Application.Abstractions.Assignments;
using Academy.Application.Contracts.Assignments;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/assignments")]
public sealed class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly IAssignmentAttachmentService _attachmentService;

    public AssignmentsController(
        IAssignmentService assignmentService,
        IAssignmentAttachmentService attachmentService)
    {
        _assignmentService = assignmentService;
        _attachmentService = attachmentService;
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<AssignmentDto>> Create(
        [FromBody] CreateAssignmentRequest request,
        CancellationToken ct)
    {
        var assignment = await _assignmentService.CreateAsync(request, ct);
        return Ok(assignment);
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<AssignmentDto>>> List(
        [FromQuery] Guid? groupId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var assignments = await _assignmentService.ListForStaffAsync(groupId, from, to, request, ct);
        return Ok(assignments);
    }

    [HttpPost("{id:guid}/attachments")]
    [Authorize(Policy = Policies.Staff)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AssignmentAttachmentDto>> UploadAttachment(
        Guid id,
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        var attachment = await _attachmentService.UploadAsync(id, file, ct);
        return Ok(attachment);
    }
}
