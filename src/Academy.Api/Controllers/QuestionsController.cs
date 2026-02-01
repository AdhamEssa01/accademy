using Academy.Application.Abstractions.Questions;
using Academy.Application.Contracts.Questions;
using Academy.Domain;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/questions")]
public sealed class QuestionsController : ControllerBase
{
    private readonly IQuestionBankService _questionBankService;

    public QuestionsController(IQuestionBankService questionBankService)
    {
        _questionBankService = questionBankService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<PagedResponse<QuestionDto>>> List(
        [FromQuery] Guid? programId,
        [FromQuery] Guid? courseId,
        [FromQuery] Guid? levelId,
        [FromQuery] QuestionType? type,
        [FromQuery] QuestionDifficulty? difficulty,
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var response = await _questionBankService.ListAsync(programId, courseId, levelId, type, difficulty, request, ct);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<QuestionDto>> Get(Guid id, CancellationToken ct)
    {
        var question = await _questionBankService.GetAsync(id, ct);
        return Ok(question);
    }

    [HttpPost]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<QuestionDto>> Create(
        [FromBody] CreateQuestionRequest request,
        CancellationToken ct)
    {
        var question = await _questionBankService.CreateAsync(request, ct);
        return Ok(question);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<ActionResult<QuestionDto>> Update(
        Guid id,
        [FromBody] UpdateQuestionRequest request,
        CancellationToken ct)
    {
        var question = await _questionBankService.UpdateAsync(id, request, ct);
        return Ok(question);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Staff)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _questionBankService.DeleteAsync(id, ct);
        return NoContent();
    }
}
