using Academy.Application.Contracts.Students;

namespace Academy.Application.Abstractions.Parents;

public interface IParentPortalService
{
    Task<IReadOnlyList<StudentDto>> GetMyChildrenAsync(CancellationToken ct);
}
