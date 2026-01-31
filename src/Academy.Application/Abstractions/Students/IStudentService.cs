using Academy.Application.Contracts.Students;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Students;

public interface IStudentService
{
    Task<PagedResponse<StudentDto>> ListAsync(PagedRequest request, CancellationToken ct);

    Task<StudentDto> GetAsync(Guid id, CancellationToken ct);

    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);

    Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct);

    Task DeleteAsync(Guid id, CancellationToken ct);
}
