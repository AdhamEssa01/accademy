using Academy.Application.Contracts.Questions;
using Academy.Domain;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Questions;

public interface IQuestionBankService
{
    Task<PagedResponse<QuestionDto>> ListAsync(
        Guid? programId,
        Guid? courseId,
        Guid? levelId,
        QuestionType? type,
        QuestionDifficulty? difficulty,
        PagedRequest request,
        CancellationToken ct);

    Task<QuestionDto> GetAsync(Guid id, CancellationToken ct);

    Task<QuestionDto> CreateAsync(CreateQuestionRequest request, CancellationToken ct);

    Task<QuestionDto> UpdateAsync(Guid id, UpdateQuestionRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);
}
