using Academy.Application.Contracts.Assignments;
using Microsoft.AspNetCore.Http;

namespace Academy.Application.Abstractions.Assignments;

public interface IAssignmentAttachmentService
{
    Task<AssignmentAttachmentDto> UploadAsync(Guid assignmentId, IFormFile file, CancellationToken ct);
}
