using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Exams;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ExamAssignmentService : IExamAssignmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public ExamAssignmentService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<ExamAssignmentDto> CreateAsync(Guid examId, CreateExamAssignmentRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var examExists = await _dbContext.Exams.AnyAsync(e => e.Id == examId, ct);
        if (!examExists)
        {
            throw new NotFoundException();
        }

        if (request.GroupId.HasValue)
        {
            var groupExists = await _dbContext.Groups.AnyAsync(g => g.Id == request.GroupId.Value, ct);
            if (!groupExists)
            {
                throw new NotFoundException();
            }
        }

        if (request.StudentId.HasValue)
        {
            var studentExists = await _dbContext.Students.AnyAsync(s => s.Id == request.StudentId.Value, ct);
            if (!studentExists)
            {
                throw new NotFoundException();
            }
        }

        if (request.CloseAtUtc < request.OpenAtUtc)
        {
            throw new ArgumentException("CloseAtUtc must be after OpenAtUtc.");
        }

        var assignment = new ExamAssignment
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            ExamId = examId,
            GroupId = request.GroupId,
            StudentId = request.StudentId,
            OpenAtUtc = request.OpenAtUtc,
            CloseAtUtc = request.CloseAtUtc,
            AttemptsAllowed = request.AttemptsAllowed,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.ExamAssignments.Add(assignment);
        await _dbContext.SaveChangesAsync(ct);

        return Map(assignment);
    }

    public async Task<IReadOnlyList<ExamAssignmentDto>> ListAsync(Guid examId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var examExists = await _dbContext.Exams.AnyAsync(e => e.Id == examId, ct);
        if (!examExists)
        {
            throw new NotFoundException();
        }

        return await _dbContext.ExamAssignments
            .AsNoTracking()
            .Where(a => a.ExamId == examId)
            .OrderBy(a => a.OpenAtUtc)
            .Select(a => new ExamAssignmentDto
            {
                Id = a.Id,
                AcademyId = a.AcademyId,
                ExamId = a.ExamId,
                GroupId = a.GroupId,
                StudentId = a.StudentId,
                OpenAtUtc = a.OpenAtUtc,
                CloseAtUtc = a.CloseAtUtc,
                AttemptsAllowed = a.AttemptsAllowed,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToListAsync(ct);
    }

    private static ExamAssignmentDto Map(ExamAssignment assignment)
        => new()
        {
            Id = assignment.Id,
            AcademyId = assignment.AcademyId,
            ExamId = assignment.ExamId,
            GroupId = assignment.GroupId,
            StudentId = assignment.StudentId,
            OpenAtUtc = assignment.OpenAtUtc,
            CloseAtUtc = assignment.CloseAtUtc,
            AttemptsAllowed = assignment.AttemptsAllowed,
            CreatedAtUtc = assignment.CreatedAtUtc
        };
}
