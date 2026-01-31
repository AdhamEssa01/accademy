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
[Route("api/v{version:apiVersion}/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IStudentPhotoService _studentPhotoService;

    public StudentsController(IStudentService studentService, IStudentPhotoService studentPhotoService)
    {
        _studentService = studentService;
        _studentPhotoService = studentPhotoService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<StudentDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var students = await _studentService.ListAsync(request, ct);
        return Ok(students);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<StudentDto>> Get(Guid id, CancellationToken ct)
    {
        var student = await _studentService.GetAsync(id, ct);
        return Ok(student);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<StudentDto>> Create(
        [FromBody] CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = await _studentService.CreateAsync(request, ct);
        return Ok(student);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<StudentDto>> Update(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken ct)
    {
        var student = await _studentService.UpdateAsync(id, request, ct);
        return Ok(student);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _studentService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/photo")]
    [Authorize(Policy = Policies.Staff)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StudentDto>> UploadPhoto(
        Guid id,
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        var student = await _studentPhotoService.UploadAsync(id, file, ct);
        return Ok(student);
    }
}
