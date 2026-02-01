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
[Route("api/v{version:apiVersion}/parent/me/evaluations")]
public sealed class ParentEvaluationsController : ControllerBase
{
    private readonly IEvaluationService _evaluationService;

    public ParentEvaluationsController(IEvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Parent)]
    public async Task<ActionResult<PagedResponse<EvaluationDto>>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var evaluations = await _evaluationService.ParentListMyChildrenAsync(from, to, request, ct);
        return Ok(evaluations);
    }
}
