using Academy.Application.Abstractions.Enrollments;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Enrollments;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public EnrollmentService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var studentExists = await _dbContext.Students
            .AnyAsync(s => s.Id == request.StudentId, ct);
        if (!studentExists)
        {
            throw new NotFoundException();
        }

        var groupExists = await _dbContext.Groups
            .AnyAsync(g => g.Id == request.GroupId, ct);
        if (!groupExists)
        {
            throw new NotFoundException();
        }

        var existing = await _dbContext.Enrollments
            .AnyAsync(e => e.StudentId == request.StudentId
                && e.GroupId == request.GroupId
                && e.EndDate == null, ct);
        if (existing)
        {
            throw new ArgumentException("Student already has an active enrollment in this group.");
        }

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            StudentId = request.StudentId,
            GroupId = request.GroupId,
            StartDate = request.StartDate,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Enrollments.Add(enrollment);
        await _dbContext.SaveChangesAsync(ct);

        return Map(enrollment);
    }

    public async Task<EnrollmentDto> EndAsync(Guid enrollmentId, EndEnrollmentRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var enrollment = await _dbContext.Enrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);

        if (enrollment is null)
        {
            throw new NotFoundException();
        }

        if (request.EndDate < enrollment.StartDate)
        {
            throw new ArgumentException("End date cannot be earlier than start date.");
        }

        enrollment.EndDate = request.EndDate;
        await _dbContext.SaveChangesAsync(ct);

        return Map(enrollment);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> ListByStudentAsync(Guid studentId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var studentExists = await _dbContext.Students
            .AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists)
        {
            throw new NotFoundException();
        }

        var query = _dbContext.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.StartDate)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                AcademyId = e.AcademyId,
                StudentId = e.StudentId,
                GroupId = e.GroupId,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                CreatedAtUtc = e.CreatedAtUtc
            });

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnrollmentDto>> ListByGroupAsync(Guid groupId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var groupExists = await _dbContext.Groups
            .AnyAsync(g => g.Id == groupId, ct);
        if (!groupExists)
        {
            throw new NotFoundException();
        }

        var query = _dbContext.Enrollments
            .AsNoTracking()
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.StartDate)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                AcademyId = e.AcademyId,
                StudentId = e.StudentId,
                GroupId = e.GroupId,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                CreatedAtUtc = e.CreatedAtUtc
            });

        return await query.ToListAsync(ct);
    }

    private static EnrollmentDto Map(Enrollment enrollment)
        => new()
        {
            Id = enrollment.Id,
            AcademyId = enrollment.AcademyId,
            StudentId = enrollment.StudentId,
            GroupId = enrollment.GroupId,
            StartDate = enrollment.StartDate,
            EndDate = enrollment.EndDate,
            CreatedAtUtc = enrollment.CreatedAtUtc
        };
}
