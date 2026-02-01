using Academy.Application.Abstractions.Assignments;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Assignments;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public AssignmentService(AppDbContext dbContext, ITenantGuard tenantGuard, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<AssignmentDto> CreateAsync(CreateAssignmentRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        var groupExists = await _dbContext.Groups
            .AnyAsync(g => g.Id == request.GroupId, ct);
        if (!groupExists)
        {
            throw new NotFoundException();
        }

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            GroupId = request.GroupId,
            Title = request.Title,
            Description = request.Description,
            DueAtUtc = request.DueAtUtc,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Assignments.Add(assignment);

        var targetStudentIds = request.TargetStudentIds
            ?.Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (targetStudentIds is { Length: > 0 })
        {
            var existingStudents = await _dbContext.Students
                .Where(s => targetStudentIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync(ct);

            if (existingStudents.Count != targetStudentIds.Length)
            {
                throw new NotFoundException();
            }

            foreach (var studentId in targetStudentIds)
            {
                _dbContext.AssignmentTargets.Add(new AssignmentTarget
                {
                    Id = Guid.NewGuid(),
                    AcademyId = academyId,
                    AssignmentId = assignment.Id,
                    StudentId = studentId,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }
        else
        {
            _dbContext.AssignmentTargets.Add(new AssignmentTarget
            {
                Id = Guid.NewGuid(),
                AcademyId = academyId,
                AssignmentId = assignment.Id,
                StudentId = null,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync(ct);

        return new AssignmentDto
        {
            Id = assignment.Id,
            AcademyId = assignment.AcademyId,
            GroupId = assignment.GroupId,
            Title = assignment.Title,
            Description = assignment.Description,
            DueAtUtc = assignment.DueAtUtc,
            CreatedByUserId = assignment.CreatedByUserId,
            CreatedAtUtc = assignment.CreatedAtUtc,
            IsGroupWide = targetStudentIds is not { Length: > 0 },
            TargetStudentIds = targetStudentIds ?? Array.Empty<Guid>(),
            Attachments = Array.Empty<AssignmentAttachmentDto>()
        };
    }

    public async Task<PagedResponse<AssignmentDto>> ListForStaffAsync(
        Guid? groupId,
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Assignments.AsNoTracking();

        if (groupId.HasValue)
        {
            query = query.Where(a => a.GroupId == groupId.Value);
        }

        if (from.HasValue)
        {
            var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.DueAtUtc.HasValue && a.DueAtUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(a => a.DueAtUtc.HasValue && a.DueAtUtc <= toUtc);
        }

        var page = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        return await MapPagedAsync(page, ct);
    }

    public async Task<PagedResponse<AssignmentDto>> ListForParentAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        var guardianId = await _dbContext.Guardians
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .Select(g => g.Id)
            .FirstOrDefaultAsync(ct);

        if (guardianId == Guid.Empty)
        {
            return new PagedResponse<AssignmentDto>(Array.Empty<AssignmentDto>(), request.Page, request.PageSize, 0);
        }

        var studentIds = await _dbContext.StudentGuardians
            .AsNoTracking()
            .Where(sg => sg.GuardianId == guardianId)
            .Select(sg => sg.StudentId)
            .Distinct()
            .ToListAsync(ct);

        if (studentIds.Count == 0)
        {
            return new PagedResponse<AssignmentDto>(Array.Empty<AssignmentDto>(), request.Page, request.PageSize, 0);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var groupIds = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId)
                && (e.EndDate == null || e.EndDate >= today))
            .Select(e => e.GroupId)
            .Distinct()
            .ToListAsync(ct);

        IQueryable<Guid> assignmentIds = _dbContext.AssignmentTargets
            .AsNoTracking()
            .Where(t => t.StudentId.HasValue && studentIds.Contains(t.StudentId.Value))
            .Select(t => t.AssignmentId);

        if (groupIds.Count > 0)
        {
            var groupWideIds = from target in _dbContext.AssignmentTargets.AsNoTracking()
                               join assignment in _dbContext.Assignments.AsNoTracking()
                                   on target.AssignmentId equals assignment.Id
                               where target.StudentId == null && groupIds.Contains(assignment.GroupId)
                               select assignment.Id;

            assignmentIds = assignmentIds.Union(groupWideIds);
        }

        var query = _dbContext.Assignments
            .AsNoTracking()
            .Where(a => assignmentIds.Contains(a.Id));

        if (from.HasValue)
        {
            var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(a => a.DueAtUtc.HasValue && a.DueAtUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(a => a.DueAtUtc.HasValue && a.DueAtUtc <= toUtc);
        }

        var page = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        return await MapPagedAsync(page, ct);
    }

    private async Task<PagedResponse<AssignmentDto>> MapPagedAsync(PagedResponse<Assignment> page, CancellationToken ct)
    {
        if (page.Items.Count == 0)
        {
            return new PagedResponse<AssignmentDto>(Array.Empty<AssignmentDto>(), page.Page, page.PageSize, page.Total);
        }

        var assignmentIds = page.Items.Select(a => a.Id).ToArray();

        var attachments = await _dbContext.AssignmentAttachments
            .AsNoTracking()
            .Where(a => assignmentIds.Contains(a.AssignmentId))
            .ToListAsync(ct);

        var targets = await _dbContext.AssignmentTargets
            .AsNoTracking()
            .Where(t => assignmentIds.Contains(t.AssignmentId))
            .ToListAsync(ct);

        var attachmentLookup = attachments
            .GroupBy(a => a.AssignmentId)
            .ToDictionary(g => g.Key, g => g.Select(MapAttachment).ToList());

        var targetLookup = targets
            .GroupBy(t => t.AssignmentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var mapped = page.Items.Select(assignment =>
        {
            var targetItems = targetLookup.TryGetValue(assignment.Id, out var t) ? t : new List<AssignmentTarget>();
            var targetIds = targetItems
                .Where(x => x.StudentId.HasValue)
                .Select(x => x.StudentId!.Value)
                .ToList();

            var isGroupWide = targetItems.Any(x => x.StudentId == null) || targetIds.Count == 0;
            var attachmentItems = attachmentLookup.TryGetValue(assignment.Id, out var a)
                ? a
                : new List<AssignmentAttachmentDto>();

            return new AssignmentDto
            {
                Id = assignment.Id,
                AcademyId = assignment.AcademyId,
                GroupId = assignment.GroupId,
                Title = assignment.Title,
                Description = assignment.Description,
                DueAtUtc = assignment.DueAtUtc,
                CreatedByUserId = assignment.CreatedByUserId,
                CreatedAtUtc = assignment.CreatedAtUtc,
                IsGroupWide = isGroupWide,
                TargetStudentIds = targetIds,
                Attachments = attachmentItems
            };
        }).ToList();

        return new PagedResponse<AssignmentDto>(mapped, page.Page, page.PageSize, page.Total);
    }

    private static AssignmentAttachmentDto MapAttachment(AssignmentAttachment attachment)
        => new()
        {
            Id = attachment.Id,
            FileUrl = attachment.FileUrl,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            CreatedAtUtc = attachment.CreatedAtUtc
        };
}
