using Academy.Application.Abstractions.Exams;
using Academy.Application.Contracts.Exams;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/exams")]
public sealed class ExamsController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<ExamDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _examService.ListAsync(request, ct);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<ExamDto>> Get(Guid id, CancellationToken ct)
    {
        var exam = await _examService.GetAsync(id, ct);
        return Ok(exam);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<ExamDto>> Create(
        [FromBody] CreateExamRequest request,
        CancellationToken ct)
    {
        var exam = await _examService.CreateAsync(request, ct);
        return Ok(exam);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<ExamDto>> Update(
        Guid id,
        [FromBody] UpdateExamRequest request,
        CancellationToken ct)
    {
        var exam = await _examService.UpdateAsync(id, request, ct);
        return Ok(exam);
    }

    [HttpPut("{id:guid}/questions")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<IReadOnlyList<ExamQuestionDto>>> UpdateQuestions(
        Guid id,
        [FromBody] UpdateExamQuestionsRequest request,
        CancellationToken ct)
    {
        var items = await _examService.UpdateQuestionsAsync(id, request, ct);
        return Ok(items);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _examService.DeleteAsync(id, ct);
        return NoContent();
    }
}
