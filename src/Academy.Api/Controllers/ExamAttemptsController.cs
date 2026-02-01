using Academy.Application.Abstractions.Exams;
using Academy.Application.Contracts.Exams;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class ExamAttemptsController : ControllerBase
{
    private readonly IExamAttemptService _examAttemptService;

    public ExamAttemptsController(IExamAttemptService examAttemptService)
    {
        _examAttemptService = examAttemptService;
    }

    [HttpPost("assignments/{assignmentId:guid}/attempts/start")]
    [Authorize(Policy = Policies.AnyAuthenticated)]
    public async Task<ActionResult<ExamAttemptDto>> Start(
        Guid assignmentId,
        [FromBody] StartExamAttemptRequest request,
        CancellationToken ct)
    {
        var attempt = await _examAttemptService.StartAsync(assignmentId, request, ct);
        return Ok(attempt);
    }

    [HttpPut("attempts/{attemptId:guid}/answers")]
    [Authorize(Policy = Policies.AnyAuthenticated)]
    public async Task<IActionResult> SaveAnswers(
        Guid attemptId,
        [FromBody] SaveAttemptAnswersRequest request,
        CancellationToken ct)
    {
        await _examAttemptService.SaveAnswersAsync(attemptId, request, ct);
        return NoContent();
    }

    [HttpPost("attempts/{attemptId:guid}/submit")]
    [Authorize(Policy = Policies.AnyAuthenticated)]
    public async Task<ActionResult<ExamAttemptDto>> Submit(
        Guid attemptId,
        CancellationToken ct)
    {
        var attempt = await _examAttemptService.SubmitAsync(attemptId, ct);
        return Ok(attempt);
    }
}
