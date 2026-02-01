using Academy.Application.Abstractions.Evaluations;
using Academy.Application.Contracts.Evaluations;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/evaluations")]
public sealed class EvaluationsController : ControllerBase
{
    private readonly IEvaluationService _evaluationService;

    public EvaluationsController(IEvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<EvaluationDto>> Create(
        [FromBody] CreateEvaluationRequest request,
        CancellationToken ct)
    {
        var evaluation = await _evaluationService.CreateAsync(request, ct);
        return Ok(evaluation);
    }

    [HttpGet("/api/v{version:apiVersion}/students/{studentId:guid}/evaluations")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<EvaluationDto>>> ListForStudent(
        Guid studentId,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var evaluations = await _evaluationService.ListForStudentAsync(studentId, request, ct);
        return Ok(evaluations);
    }
}
