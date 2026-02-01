using Academy.Application.Abstractions.Exams;
using Academy.Application.Contracts.Exams;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/attempt-answers")]
public sealed class AttemptAnswersController : ControllerBase
{
    private readonly IExamManualGradingService _examManualGradingService;

    public AttemptAnswersController(IExamManualGradingService examManualGradingService)
    {
        _examManualGradingService = examManualGradingService;
    }

    [HttpPost("{id:guid}/grade")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<IActionResult> Grade(
        Guid id,
        [FromBody] GradeAttemptAnswerRequest request,
        CancellationToken ct)
    {
        await _examManualGradingService.GradeAnswerAsync(id, request, ct);
        return NoContent();
    }
}
