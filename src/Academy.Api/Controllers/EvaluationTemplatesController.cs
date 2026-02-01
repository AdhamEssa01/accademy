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
[Route("api/v{version:apiVersion}/evaluation-templates")]
public sealed class EvaluationTemplatesController : ControllerBase
{
    private readonly IEvaluationTemplateService _templateService;

    public EvaluationTemplatesController(IEvaluationTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<EvaluationTemplateDto>>> List(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var templates = await _templateService.ListAsync(request, ct);
        return Ok(templates);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<EvaluationTemplateDto>> Get(Guid id, CancellationToken ct)
    {
        var template = await _templateService.GetAsync(id, ct);
        return Ok(template);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<EvaluationTemplateDto>> Create(
        [FromBody] CreateEvaluationTemplateRequest request,
        CancellationToken ct)
    {
        var template = await _templateService.CreateAsync(request, ct);
        return Ok(template);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<EvaluationTemplateDto>> Update(
        Guid id,
        [FromBody] UpdateEvaluationTemplateRequest request,
        CancellationToken ct)
    {
        var template = await _templateService.UpdateAsync(id, request, ct);
        return Ok(template);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _templateService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/criteria")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<PagedResponse<RubricCriterionDto>>> ListCriteria(
        Guid id,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var criteria = await _templateService.ListCriteriaAsync(id, request, ct);
        return Ok(criteria);
    }

    [HttpPost("{id:guid}/criteria")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<RubricCriterionDto>> CreateCriterion(
        Guid id,
        [FromBody] CreateRubricCriterionRequest request,
        CancellationToken ct)
    {
        var criterion = await _templateService.CreateCriterionAsync(id, request, ct);
        return Ok(criterion);
    }

    [HttpPut("{id:guid}/criteria/{criterionId:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<RubricCriterionDto>> UpdateCriterion(
        Guid id,
        Guid criterionId,
        [FromBody] UpdateRubricCriterionRequest request,
        CancellationToken ct)
    {
        var criterion = await _templateService.UpdateCriterionAsync(id, criterionId, request, ct);
        return Ok(criterion);
    }

    [HttpDelete("{id:guid}/criteria/{criterionId:guid}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<IActionResult> DeleteCriterion(
        Guid id,
        Guid criterionId,
        CancellationToken ct)
    {
        await _templateService.DeleteCriterionAsync(id, criterionId, ct);
        return NoContent();
    }
}
