using Academy.Application.Abstractions.Exams;
using Academy.Application.Contracts.Exams;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/exams/{examId:guid}/stats")]
public sealed class ExamStatsController : ControllerBase
{
    private readonly IExamAnalyticsService _examAnalyticsService;

    public ExamStatsController(IExamAnalyticsService examAnalyticsService)
    {
        _examAnalyticsService = examAnalyticsService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<ExamStatsDto>> Get(Guid examId, CancellationToken ct)
    {
        var stats = await _examAnalyticsService.GetStatsAsync(examId, ct);
        return Ok(stats);
    }
}
