using Academy.Application.Contracts.Students;
using Microsoft.AspNetCore.Http;

namespace Academy.Application.Abstractions.Students;

public interface IStudentPhotoService
{
    Task<StudentDto> UploadAsync(Guid studentId, IFormFile file, CancellationToken ct);
}
