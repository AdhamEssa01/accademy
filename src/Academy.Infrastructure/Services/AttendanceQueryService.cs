using Academy.Application.Abstractions.Attendance;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Attendance;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AttendanceQueryService : IAttendanceQueryService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public AttendanceQueryService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResponse<AttendanceRecordDto>> ListAsync(
        Guid? groupId,
        Guid? studentId,
        DateOnly? from,
        DateOnly? to,
        AttendanceStatus? status,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.AttendanceRecords.AsNoTracking();
        var sessions = _dbContext.Sessions.AsNoTracking();

        query = ApplySessionFilters(query, sessions, groupId, from, to);

        if (studentId.HasValue)
        {
            query = query.Where(a => a.StudentId == studentId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var projected = query
            .OrderByDescending(a => a.MarkedAtUtc)
            .Select(a => new AttendanceRecordDto
            {
                Id = a.Id,
                AcademyId = a.AcademyId,
                SessionId = a.SessionId,
                StudentId = a.StudentId,
                Status = a.Status,
                Reason = a.Reason,
                Note = a.Note,
                MarkedByUserId = a.MarkedByUserId,
                MarkedAtUtc = a.MarkedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<PagedResponse<AttendanceRecordDto>> ParentListForMyChildrenAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        if (_currentUserContext.UserId is null)
        {
            throw new ForbiddenException();
        }

        var guardianId = await _dbContext.Guardians
            .AsNoTracking()
            .Where(g => g.UserId == _currentUserContext.UserId)
            .Select(g => g.Id)
            .FirstOrDefaultAsync(ct);

        if (guardianId == Guid.Empty)
        {
            return new PagedResponse<AttendanceRecordDto>(Array.Empty<AttendanceRecordDto>(), request.Page, request.PageSize, 0);
        }

        var studentIds = await _dbContext.StudentGuardians
            .AsNoTracking()
            .Where(sg => sg.GuardianId == guardianId)
            .Select(sg => sg.StudentId)
            .Distinct()
            .ToListAsync(ct);

        if (studentIds.Count == 0)
        {
            return new PagedResponse<AttendanceRecordDto>(Array.Empty<AttendanceRecordDto>(), request.Page, request.PageSize, 0);
        }

        var query = _dbContext.AttendanceRecords.AsNoTracking()
            .Where(a => studentIds.Contains(a.StudentId));

        query = ApplySessionFilters(query, _dbContext.Sessions.AsNoTracking(), groupId: null, from, to);

        var projected = query
            .OrderByDescending(a => a.MarkedAtUtc)
            .Select(a => new AttendanceRecordDto
            {
                Id = a.Id,
                AcademyId = a.AcademyId,
                SessionId = a.SessionId,
                StudentId = a.StudentId,
                Status = a.Status,
                Reason = a.Reason,
                Note = a.Note,
                MarkedByUserId = a.MarkedByUserId,
                MarkedAtUtc = a.MarkedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    private static IQueryable<AttendanceRecord> ApplySessionFilters(
        IQueryable<AttendanceRecord> query,
        IQueryable<Session> sessions,
        Guid? groupId,
        DateOnly? from,
        DateOnly? to)
    {
        if (groupId.HasValue || from.HasValue || to.HasValue)
        {
            if (groupId.HasValue)
            {
                sessions = sessions.Where(s => s.GroupId == groupId.Value);
            }

            if (from.HasValue)
            {
                var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue);
                sessions = sessions.Where(s => s.StartsAtUtc >= fromUtc);
            }

            if (to.HasValue)
            {
                var toUtc = to.Value.ToDateTime(TimeOnly.MaxValue);
                sessions = sessions.Where(s => s.StartsAtUtc <= toUtc);
            }

            var sessionIds = sessions.Select(s => s.Id);
            query = query.Where(a => sessionIds.Contains(a.SessionId));
        }

        return query;
    }
}
