using Academy.Application.Abstractions.Attendance;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Attendance;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public AttendanceService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> SubmitForSessionAsync(
        Guid sessionId,
        SubmitAttendanceRequest request,
        CancellationToken ct)
    {
        var session = await GetSessionOrThrowAsync(sessionId, ct);
        var academyId = _tenantGuard.GetAcademyIdOrThrow();
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        var studentIds = request.Items
            .Select(item => item.StudentId)
            .Distinct()
            .ToArray();

        var existingStudents = await _dbContext.Students
            .Where(s => studentIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (existingStudents.Count != studentIds.Length)
        {
            throw new NotFoundException();
        }

        var existingRecords = await _dbContext.AttendanceRecords
            .Where(a => a.SessionId == session.Id && studentIds.Contains(a.StudentId))
            .ToListAsync(ct);

        foreach (var item in request.Items)
        {
            var record = existingRecords.FirstOrDefault(a => a.StudentId == item.StudentId);
            if (record is null)
            {
                record = new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    AcademyId = academyId,
                    SessionId = session.Id,
                    StudentId = item.StudentId,
                    Status = item.Status,
                    Reason = item.Reason,
                    Note = item.Note,
                    MarkedByUserId = userId,
                    MarkedAtUtc = DateTime.UtcNow
                };

                _dbContext.AttendanceRecords.Add(record);
                existingRecords.Add(record);
            }
            else
            {
                record.Status = item.Status;
                record.Reason = item.Reason;
                record.Note = item.Note;
                record.MarkedByUserId = userId;
                record.MarkedAtUtc = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(ct);

        return existingRecords
            .OrderBy(r => r.StudentId)
            .Select(Map)
            .ToList();
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> ListForSessionAsync(Guid sessionId, CancellationToken ct)
    {
        await GetSessionOrThrowAsync(sessionId, ct);

        return await _dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.StudentId)
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
            })
            .ToListAsync(ct);
    }

    private async Task<Session> GetSessionOrThrowAsync(Guid sessionId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

        if (session is null)
        {
            throw new NotFoundException();
        }

        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();
        var roles = _currentUserContext.Roles;

        if (roles.Contains(Roles.Admin))
        {
            return session;
        }

        if (roles.Contains(Roles.Instructor) && session.InstructorUserId == userId)
        {
            return session;
        }

        throw new ForbiddenException();
    }

    private static AttendanceRecordDto Map(AttendanceRecord record)
        => new()
        {
            Id = record.Id,
            AcademyId = record.AcademyId,
            SessionId = record.SessionId,
            StudentId = record.StudentId,
            Status = record.Status,
            Reason = record.Reason,
            Note = record.Note,
            MarkedByUserId = record.MarkedByUserId,
            MarkedAtUtc = record.MarkedAtUtc
        };
}
