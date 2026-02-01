using Academy.Application.Abstractions.Behavior;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Behavior;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class BehaviorService : IBehaviorService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public BehaviorService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<BehaviorEventDto> CreateAsync(CreateBehaviorEventRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        var studentExists = await _dbContext.Students.AnyAsync(s => s.Id == request.StudentId, ct);
        if (!studentExists)
        {
            throw new NotFoundException();
        }

        if (request.SessionId.HasValue)
        {
            var sessionExists = await _dbContext.Sessions.AnyAsync(s => s.Id == request.SessionId.Value, ct);
            if (!sessionExists)
            {
                throw new NotFoundException();
            }
        }

        var behaviorEvent = new BehaviorEvent
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            StudentId = request.StudentId,
            SessionId = request.SessionId,
            Type = request.Type,
            Points = request.Points,
            Reason = request.Reason,
            Note = request.Note,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.BehaviorEvents.Add(behaviorEvent);
        await _dbContext.SaveChangesAsync(ct);

        return Map(behaviorEvent);
    }

    public async Task<PagedResponse<BehaviorEventDto>> ListForStudentAsync(
        Guid studentId,
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var studentExists = await _dbContext.Students.AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists)
        {
            throw new NotFoundException();
        }

        var query = _dbContext.BehaviorEvents
            .AsNoTracking()
            .Where(b => b.StudentId == studentId);

        query = ApplyDateRange(query, from, to);

        var projected = query
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => new BehaviorEventDto
            {
                Id = b.Id,
                AcademyId = b.AcademyId,
                StudentId = b.StudentId,
                SessionId = b.SessionId,
                Type = b.Type,
                Points = b.Points,
                Reason = b.Reason,
                Note = b.Note,
                CreatedByUserId = b.CreatedByUserId,
                CreatedAtUtc = b.CreatedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<PagedResponse<BehaviorEventDto>> ParentListMyChildrenAsync(
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
            return new PagedResponse<BehaviorEventDto>(Array.Empty<BehaviorEventDto>(), request.Page, request.PageSize, 0);
        }

        var studentIds = await _dbContext.StudentGuardians
            .AsNoTracking()
            .Where(sg => sg.GuardianId == guardianId)
            .Select(sg => sg.StudentId)
            .Distinct()
            .ToListAsync(ct);

        if (studentIds.Count == 0)
        {
            return new PagedResponse<BehaviorEventDto>(Array.Empty<BehaviorEventDto>(), request.Page, request.PageSize, 0);
        }

        var query = _dbContext.BehaviorEvents
            .AsNoTracking()
            .Where(b => studentIds.Contains(b.StudentId));

        query = ApplyDateRange(query, from, to);

        var projected = query
            .OrderByDescending(b => b.CreatedAtUtc)
            .Select(b => new BehaviorEventDto
            {
                Id = b.Id,
                AcademyId = b.AcademyId,
                StudentId = b.StudentId,
                SessionId = b.SessionId,
                Type = b.Type,
                Points = b.Points,
                Reason = b.Reason,
                Note = b.Note,
                CreatedByUserId = b.CreatedByUserId,
                CreatedAtUtc = b.CreatedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    private static IQueryable<BehaviorEvent> ApplyDateRange(
        IQueryable<BehaviorEvent> query,
        DateOnly? from,
        DateOnly? to)
    {
        if (from.HasValue)
        {
            var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(b => b.CreatedAtUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(b => b.CreatedAtUtc <= toUtc);
        }

        return query;
    }

    private static BehaviorEventDto Map(BehaviorEvent behaviorEvent)
        => new()
        {
            Id = behaviorEvent.Id,
            AcademyId = behaviorEvent.AcademyId,
            StudentId = behaviorEvent.StudentId,
            SessionId = behaviorEvent.SessionId,
            Type = behaviorEvent.Type,
            Points = behaviorEvent.Points,
            Reason = behaviorEvent.Reason,
            Note = behaviorEvent.Note,
            CreatedByUserId = behaviorEvent.CreatedByUserId,
            CreatedAtUtc = behaviorEvent.CreatedAtUtc
        };
}
