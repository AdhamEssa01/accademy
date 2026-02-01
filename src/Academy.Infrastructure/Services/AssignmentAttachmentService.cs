using Academy.Application.Abstractions.Assignments;
using Academy.Application.Abstractions.Media;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Assignments;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AssignmentAttachmentService : IAssignmentAttachmentService
{
    private const long MaxFileBytes = 2 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly IMediaStorage _mediaStorage;

    public AssignmentAttachmentService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        IMediaStorage mediaStorage)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _mediaStorage = mediaStorage;
    }

    public async Task<AssignmentAttachmentDto> UploadAsync(Guid assignmentId, IFormFile file, CancellationToken ct)
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

        var assignment = await _dbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId, ct);

        if (assignment is null)
        {
            throw new NotFoundException();
        }

        var extension = GetExtension(file.ContentType);
        var fileName = $"{assignmentId:N}_{Guid.NewGuid():N}{extension}";

        await using var stream = file.OpenReadStream();
        var relativeUrl = await _mediaStorage.SaveAsync(
            stream,
            file.ContentType,
            fileName,
            "uploads/assignments",
            ct);

        var attachment = new AssignmentAttachment
        {
            Id = Guid.NewGuid(),
            AcademyId = assignment.AcademyId,
            AssignmentId = assignmentId,
            FileUrl = relativeUrl,
            FileName = file.FileName,
            ContentType = file.ContentType,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.AssignmentAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(ct);

        return new AssignmentAttachmentDto
        {
            Id = attachment.Id,
            FileUrl = attachment.FileUrl,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            CreatedAtUtc = attachment.CreatedAtUtc
        };
    }

    private static string GetExtension(string contentType)
        => contentType.ToLowerInvariant() switch
        {
            "application/pdf" => ".pdf",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            _ => ".bin"
        };
}
