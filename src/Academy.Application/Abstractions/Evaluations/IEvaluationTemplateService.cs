using Academy.Application.Contracts.Evaluations;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Evaluations;

public interface IEvaluationTemplateService
{
    Task<PagedResponse<EvaluationTemplateDto>> ListAsync(PagedRequest request, CancellationToken ct);
    Task<EvaluationTemplateDto> GetAsync(Guid id, CancellationToken ct);
    Task<EvaluationTemplateDto> CreateAsync(CreateEvaluationTemplateRequest request, CancellationToken ct);
    Task<EvaluationTemplateDto> UpdateAsync(Guid id, UpdateEvaluationTemplateRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<RubricCriterionDto>> ListCriteriaAsync(Guid templateId, PagedRequest request, CancellationToken ct);
    Task<RubricCriterionDto> CreateCriterionAsync(Guid templateId, CreateRubricCriterionRequest request, CancellationToken ct);
    Task<RubricCriterionDto> UpdateCriterionAsync(Guid templateId, Guid criterionId, UpdateRubricCriterionRequest request, CancellationToken ct);
    Task DeleteCriterionAsync(Guid templateId, Guid criterionId, CancellationToken ct);
}
