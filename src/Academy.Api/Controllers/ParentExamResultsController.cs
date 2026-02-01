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
[Route("api/v{version:apiVersion}/parent/me/exam-results")]
public sealed class ParentExamResultsController : ControllerBase
{
    private readonly IExamAttemptService _examAttemptService;

    public ParentExamResultsController(IExamAttemptService examAttemptService)
    {
        _examAttemptService = examAttemptService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<PagedResponse<ExamResultDto>>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _examAttemptService.ParentListMyChildrenAsync(from, to, request, ct);
        return Ok(response);
    }
}
