using Academy.Application.Abstractions.Media;
using Academy.Application.Abstractions.Security;
using Academy.Application.Abstractions.Students;
using Academy.Application.Contracts.Students;
using Academy.Application.Exceptions;
using Academy.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class StudentPhotoService : IStudentPhotoService
{
    private const long MaxFileBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly IMediaStorage _mediaStorage;

    public StudentPhotoService(AppDbContext dbContext, ITenantGuard tenantGuard, IMediaStorage mediaStorage)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _mediaStorage = mediaStorage;
    }

    public async Task<StudentDto> UploadAsync(Guid studentId, IFormFile file, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("File is required.");
        }

        if (file.Length > MaxFileBytes)
        {
            throw new ArgumentException("File size exceeds 2 MB.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("Unsupported file type.");
        }

        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == studentId, ct);

        if (student is null)
        {
            throw new NotFoundException();
        }

        var extension = GetExtension(file.ContentType);
        var fileName = $"{studentId:N}_{Guid.NewGuid():N}{extension}";

        await using var stream = file.OpenReadStream();
        var relativeUrl = await _mediaStorage.SaveAsync(
            stream,
            file.ContentType,
            fileName,
            "uploads/students",
            ct);

        student.PhotoUrl = relativeUrl;
        await _dbContext.SaveChangesAsync(ct);

        return new StudentDto
        {
            Id = student.Id,
            AcademyId = student.AcademyId,
            FullName = student.FullName,
            DateOfBirth = student.DateOfBirth,
            PhotoUrl = student.PhotoUrl,
            Notes = student.Notes,
            CreatedAtUtc = student.CreatedAtUtc
        };
    }

    private static string GetExtension(string contentType)
        => contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".bin"
        };
}
