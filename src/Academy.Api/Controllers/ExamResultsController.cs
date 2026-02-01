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
[Route("api/v{version:apiVersion}/exams/{examId:guid}/results")]
public sealed class ExamResultsController : ControllerBase
{
    private readonly IExamAttemptService _examAttemptService;

    public ExamResultsController(IExamAttemptService examAttemptService)
    {
        _examAttemptService = examAttemptService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<ExamResultDto>>> List(
        Guid examId,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _examAttemptService.ListForExamAsync(examId, request, ct);
        return Ok(response);
    }
}
