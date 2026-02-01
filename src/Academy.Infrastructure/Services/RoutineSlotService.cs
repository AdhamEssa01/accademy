using Academy.Application.Abstractions.Scheduling;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.RoutineSlots;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class RoutineSlotService : IRoutineSlotService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public RoutineSlotService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<RoutineSlotDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.RoutineSlots
            .AsNoTracking()
            .OrderBy(r => r.DayOfWeek)
            .ThenBy(r => r.StartTime)
            .Select(r => new RoutineSlotDto
            {
                Id = r.Id,
                AcademyId = r.AcademyId,
                GroupId = r.GroupId,
                DayOfWeek = r.DayOfWeek,
                StartTime = r.StartTime,
                DurationMinutes = r.DurationMinutes,
                InstructorUserId = r.InstructorUserId,
                CreatedAtUtc = r.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<RoutineSlotDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var slot = await _dbContext.RoutineSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (slot is null)
        {
            throw new NotFoundException();
        }

        return Map(slot);
    }

    public async Task<RoutineSlotDto> CreateAsync(CreateRoutineSlotRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var groupExists = await _dbContext.Groups
            .AnyAsync(g => g.Id == request.GroupId, ct);
        if (!groupExists)
        {
            throw new NotFoundException();
        }

        var instructorExists = await _dbContext.Users
            .AnyAsync(u => u.Id == request.InstructorUserId, ct);
        if (!instructorExists)
        {
            throw new NotFoundException();
        }

        var slot = new RoutineSlot
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            GroupId = request.GroupId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            DurationMinutes = request.DurationMinutes,
            InstructorUserId = request.InstructorUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.RoutineSlots.Add(slot);
        await _dbContext.SaveChangesAsync(ct);

        return Map(slot);
    }

    public async Task<RoutineSlotDto> UpdateAsync(Guid id, UpdateRoutineSlotRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var slot = await _dbContext.RoutineSlots
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (slot is null)
        {
            throw new NotFoundException();
        }

        var groupExists = await _dbContext.Groups
            .AnyAsync(g => g.Id == request.GroupId, ct);
        if (!groupExists)
        {
            throw new NotFoundException();
        }

        var instructorExists = await _dbContext.Users
            .AnyAsync(u => u.Id == request.InstructorUserId, ct);
        if (!instructorExists)
        {
            throw new NotFoundException();
        }

        slot.GroupId = request.GroupId;
        slot.DayOfWeek = request.DayOfWeek;
        slot.StartTime = request.StartTime;
        slot.DurationMinutes = request.DurationMinutes;
        slot.InstructorUserId = request.InstructorUserId;

        await _dbContext.SaveChangesAsync(ct);

        return Map(slot);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var slot = await _dbContext.RoutineSlots
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (slot is null)
        {
            throw new NotFoundException();
        }

        _dbContext.RoutineSlots.Remove(slot);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<RoutineSlotDto>> ListMineAsync(Guid instructorUserId, PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.RoutineSlots
            .AsNoTracking()
            .Where(r => r.InstructorUserId == instructorUserId)
            .OrderBy(r => r.DayOfWeek)
            .ThenBy(r => r.StartTime)
            .Select(r => new RoutineSlotDto
            {
                Id = r.Id,
                AcademyId = r.AcademyId,
                GroupId = r.GroupId,
                DayOfWeek = r.DayOfWeek,
                StartTime = r.StartTime,
                DurationMinutes = r.DurationMinutes,
                InstructorUserId = r.InstructorUserId,
                CreatedAtUtc = r.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<int> GenerateSessionsAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        if (from > to)
        {
            throw new ArgumentException("Invalid date range.");
        }

        var slots = await _dbContext.RoutineSlots
            .AsNoTracking()
            .ToListAsync(ct);

        if (slots.Count == 0)
        {
            return 0;
        }

        var groupIds = slots.Select(s => s.GroupId).Distinct().ToArray();
        var fromUtc = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(to.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var existingSessions = await _dbContext.Sessions
            .AsNoTracking()
            .Where(s => groupIds.Contains(s.GroupId) && s.StartsAtUtc >= fromUtc && s.StartsAtUtc <= toUtc)
            .Select(s => new { s.GroupId, s.StartsAtUtc })
            .ToListAsync(ct);

        var existingKeys = new HashSet<(Guid GroupId, DateTime StartsAtUtc)>(
            existingSessions.Select(s => (s.GroupId, s.StartsAtUtc)));

        var createdAt = DateTime.UtcNow;
        var newSessions = new List<Session>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            foreach (var slot in slots.Where(s => s.DayOfWeek == date.DayOfWeek))
            {
                var startsAtUtc = DateTime.SpecifyKind(date.ToDateTime(slot.StartTime), DateTimeKind.Utc);
                var key = (slot.GroupId, startsAtUtc);
                if (existingKeys.Contains(key))
                {
                    continue;
                }

                existingKeys.Add(key);
                newSessions.Add(new Session
                {
                    Id = Guid.NewGuid(),
                    AcademyId = academyId,
                    GroupId = slot.GroupId,
                    InstructorUserId = slot.InstructorUserId,
                    StartsAtUtc = startsAtUtc,
                    DurationMinutes = slot.DurationMinutes,
                    CreatedAtUtc = createdAt
                });
            }
        }

        if (newSessions.Count == 0)
        {
            return 0;
        }

        _dbContext.Sessions.AddRange(newSessions);
        await _dbContext.SaveChangesAsync(ct);

        return newSessions.Count;
    }

    private static RoutineSlotDto Map(RoutineSlot slot)
        => new()
        {
            Id = slot.Id,
            AcademyId = slot.AcademyId,
            GroupId = slot.GroupId,
            DayOfWeek = slot.DayOfWeek,
            StartTime = slot.StartTime,
            DurationMinutes = slot.DurationMinutes,
            InstructorUserId = slot.InstructorUserId,
            CreatedAtUtc = slot.CreatedAtUtc
        };
}
