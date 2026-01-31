using Academy.Application.Abstractions.Scheduling;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Sessions;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class SessionService : ISessionService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public SessionService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<SessionDto>> ListAsync(
        Guid? groupId,
        DateTime? fromUtc,
        DateTime? toUtc,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Sessions.AsNoTracking();
        if (groupId.HasValue)
        {
            query = query.Where(s => s.GroupId == groupId.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(s => s.StartsAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(s => s.StartsAtUtc <= toUtc.Value);
        }

        var projected = query
            .OrderBy(s => s.StartsAtUtc)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                AcademyId = s.AcademyId,
                GroupId = s.GroupId,
                InstructorUserId = s.InstructorUserId,
                StartsAtUtc = s.StartsAtUtc,
                DurationMinutes = s.DurationMinutes,
                Notes = s.Notes,
                CreatedAtUtc = s.CreatedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<SessionDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (session is null)
        {
            throw new NotFoundException();
        }

        return Map(session);
    }

    public async Task<SessionDto> CreateAsync(CreateSessionRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var groupExists = await _dbContext.Groups
            .AnyAsync(g => g.Id == request.GroupId, ct);
        if (!groupExists)
        {
            throw new NotFoundException();
        }

        var session = new Session
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            GroupId = request.GroupId,
            InstructorUserId = request.InstructorUserId,
            StartsAtUtc = request.StartsAtUtc,
            DurationMinutes = request.DurationMinutes,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync(ct);

        return Map(session);
    }

    public async Task<SessionDto> UpdateAsync(Guid id, UpdateSessionRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (session is null)
        {
            throw new NotFoundException();
        }

        session.InstructorUserId = request.InstructorUserId;
        session.StartsAtUtc = request.StartsAtUtc;
        session.DurationMinutes = request.DurationMinutes;
        session.Notes = request.Notes;
        await _dbContext.SaveChangesAsync(ct);

        return Map(session);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (session is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Sessions.Remove(session);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<SessionDto>> ListMineAsync(
        Guid instructorUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Sessions
            .AsNoTracking()
            .Where(s => s.InstructorUserId == instructorUserId);

        if (fromUtc.HasValue)
        {
            query = query.Where(s => s.StartsAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(s => s.StartsAtUtc <= toUtc.Value);
        }

        var projected = query
            .OrderBy(s => s.StartsAtUtc)
            .Select(s => new SessionDto
            {
                Id = s.Id,
                AcademyId = s.AcademyId,
                GroupId = s.GroupId,
                InstructorUserId = s.InstructorUserId,
                StartsAtUtc = s.StartsAtUtc,
                DurationMinutes = s.DurationMinutes,
                Notes = s.Notes,
                CreatedAtUtc = s.CreatedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    private static SessionDto Map(Session session)
        => new()
        {
            Id = session.Id,
            AcademyId = session.AcademyId,
            GroupId = session.GroupId,
            InstructorUserId = session.InstructorUserId,
            StartsAtUtc = session.StartsAtUtc,
            DurationMinutes = session.DurationMinutes,
            Notes = session.Notes,
            CreatedAtUtc = session.CreatedAtUtc
        };
}
