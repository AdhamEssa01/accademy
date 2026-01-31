using Academy.Application.Abstractions.Catalog;
using Academy.Application.Contracts.Courses;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/courses")]
public sealed class CoursesController : ControllerBase
{
    private readonly IProgramCatalogService _catalogService;

    public CoursesController(IProgramCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<CourseDto>>> List(
        [FromQuery] Guid? programId,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var courses = await _catalogService.ListCoursesAsync(programId, request, ct);
        return Ok(courses);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<CourseDto>> Get(Guid id, CancellationToken ct)
    {
        var course = await _catalogService.GetCourseAsync(id, ct);
        return Ok(course);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<CourseDto>> Create(
        [FromBody] CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = await _catalogService.CreateCourseAsync(request, ct);
        return Ok(course);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<CourseDto>> Update(
        Guid id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken ct)
    {
        var course = await _catalogService.UpdateCourseAsync(id, request, ct);
        return Ok(course);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _catalogService.DeleteCourseAsync(id, ct);
        return NoContent();
    }
}
