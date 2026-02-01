using Academy.Application.Abstractions.Announcements;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Announcements;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AnnouncementService : IAnnouncementService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public AnnouncementService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        if (request.Audience is AnnouncementAudience.GroupParents or AnnouncementAudience.GroupStaff)
        {
            if (!request.GroupId.HasValue)
            {
                throw new NotFoundException();
            }

            var groupExists = await _dbContext.Groups
                .AnyAsync(g => g.Id == request.GroupId.Value, ct);
            if (!groupExists)
            {
                throw new NotFoundException();
            }
        }

        var now = DateTime.UtcNow;
        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            Title = request.Title,
            Body = request.Body,
            Audience = request.Audience,
            GroupId = request.GroupId,
            PublishedAtUtc = now,
            CreatedByUserId = userId,
            CreatedAtUtc = now
        };

        _dbContext.Announcements.Add(announcement);

        var targetUserIds = await ResolveTargetUserIdsAsync(request, ct);
        if (targetUserIds.Count > 0)
        {
            var notifications = targetUserIds.Select(targetUserId => new Notification
            {
                Id = Guid.NewGuid(),
                AcademyId = academyId,
                UserId = targetUserId,
                AnnouncementId = announcement.Id,
                Title = announcement.Title,
                Body = announcement.Body,
                IsRead = false,
                CreatedAtUtc = now
            });

            _dbContext.Notifications.AddRange(notifications);
        }

        await _dbContext.SaveChangesAsync(ct);

        return Map(announcement);
    }

    public async Task<PagedResponse<AnnouncementDto>> ListForStaffAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Announcements
            .AsNoTracking()
            .OrderByDescending(a => a.PublishedAtUtc)
            .Select(a => new AnnouncementDto
            {
                Id = a.Id,
                AcademyId = a.AcademyId,
                Title = a.Title,
                Body = a.Body,
                Audience = a.Audience,
                GroupId = a.GroupId,
                PublishedAtUtc = a.PublishedAtUtc,
                CreatedByUserId = a.CreatedByUserId,
                CreatedAtUtc = a.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<PagedResponse<AnnouncementDto>> ListForParentAsync(PagedRequest request, CancellationToken ct)
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
            return new PagedResponse<AnnouncementDto>(Array.Empty<AnnouncementDto>(), request.Page, request.PageSize, 0);
        }

        var studentIds = await _dbContext.StudentGuardians
            .AsNoTracking()
            .Where(sg => sg.GuardianId == guardianId)
            .Select(sg => sg.StudentId)
            .Distinct()
            .ToListAsync(ct);

        if (studentIds.Count == 0)
        {
            return new PagedResponse<AnnouncementDto>(Array.Empty<AnnouncementDto>(), request.Page, request.PageSize, 0);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var groupIds = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId)
                && (e.EndDate == null || e.EndDate >= today))
            .Select(e => e.GroupId)
            .Distinct()
            .ToListAsync(ct);

        var query = _dbContext.Announcements.AsNoTracking();

        if (groupIds.Count > 0)
        {
            query = query.Where(a => a.Audience == AnnouncementAudience.AllParents
                || (a.Audience == AnnouncementAudience.GroupParents && groupIds.Contains(a.GroupId!.Value)));
        }
        else
        {
            query = query.Where(a => a.Audience == AnnouncementAudience.AllParents);
        }

        var page = await query
            .OrderByDescending(a => a.PublishedAtUtc)
            .Select(a => new AnnouncementDto
            {
                Id = a.Id,
                AcademyId = a.AcademyId,
                Title = a.Title,
                Body = a.Body,
                Audience = a.Audience,
                GroupId = a.GroupId,
                PublishedAtUtc = a.PublishedAtUtc,
                CreatedByUserId = a.CreatedByUserId,
                CreatedAtUtc = a.CreatedAtUtc
            })
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        return page;
    }

    private async Task<List<Guid>> ResolveTargetUserIdsAsync(CreateAnnouncementRequest request, CancellationToken ct)
    {
        return request.Audience switch
        {
            AnnouncementAudience.AllParents => await GetParentUserIdsAsync(ct),
            AnnouncementAudience.AllStaff => await GetStaffUserIdsAsync(ct),
            AnnouncementAudience.GroupParents => await GetGroupParentUserIdsAsync(request.GroupId, ct),
            AnnouncementAudience.GroupStaff => await GetGroupStaffUserIdsAsync(request.GroupId, ct),
            _ => new List<Guid>()
        };
    }

    private async Task<List<Guid>> GetParentUserIdsAsync(CancellationToken ct)
    {
        return await _dbContext.Guardians
            .AsNoTracking()
            .Where(g => g.UserId != null)
            .Select(g => g.UserId!.Value)
            .Distinct()
            .ToListAsync(ct);
    }

    private async Task<List<Guid>> GetStaffUserIdsAsync(CancellationToken ct)
    {
        var query = from userRole in _dbContext.UserRoles.AsNoTracking()
                    join role in _dbContext.Roles.AsNoTracking()
                        on userRole.RoleId equals role.Id
                    join profile in _dbContext.UserProfiles.AsNoTracking()
                        on userRole.UserId equals profile.UserId
                    where role.Name == Roles.Admin || role.Name == Roles.Instructor
                    select userRole.UserId;

        return await query.Distinct().ToListAsync(ct);
    }

    private async Task<List<Guid>> GetGroupParentUserIdsAsync(Guid? groupId, CancellationToken ct)
    {
        if (!groupId.HasValue)
        {
            return new List<Guid>();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var studentIds = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(e => e.GroupId == groupId.Value
                && (e.EndDate == null || e.EndDate >= today))
            .Select(e => e.StudentId)
            .Distinct()
            .ToListAsync(ct);

        if (studentIds.Count == 0)
        {
            return new List<Guid>();
        }

        var query = from link in _dbContext.StudentGuardians.AsNoTracking()
                    join guardian in _dbContext.Guardians.AsNoTracking()
                        on link.GuardianId equals guardian.Id
                    where studentIds.Contains(link.StudentId)
                        && guardian.UserId != null
                    select guardian.UserId!.Value;

        return await query.Distinct().ToListAsync(ct);
    }

    private async Task<List<Guid>> GetGroupStaffUserIdsAsync(Guid? groupId, CancellationToken ct)
    {
        if (!groupId.HasValue)
        {
            return new List<Guid>();
        }

        var adminIds = await GetAdminUserIdsAsync(ct);
        var instructorId = await _dbContext.Groups
            .AsNoTracking()
            .Where(g => g.Id == groupId.Value)
            .Select(g => g.InstructorUserId)
            .FirstOrDefaultAsync(ct);

        if (instructorId.HasValue)
        {
            adminIds.Add(instructorId.Value);
        }

        return adminIds.Distinct().ToList();
    }

    private async Task<List<Guid>> GetAdminUserIdsAsync(CancellationToken ct)
    {
        var query = from userRole in _dbContext.UserRoles.AsNoTracking()
                    join role in _dbContext.Roles.AsNoTracking()
                        on userRole.RoleId equals role.Id
                    join profile in _dbContext.UserProfiles.AsNoTracking()
                        on userRole.UserId equals profile.UserId
                    where role.Name == Roles.Admin
                    select userRole.UserId;

        return await query.Distinct().ToListAsync(ct);
    }

    private static AnnouncementDto Map(Announcement announcement)
        => new()
        {
            Id = announcement.Id,
            AcademyId = announcement.AcademyId,
            Title = announcement.Title,
            Body = announcement.Body,
            Audience = announcement.Audience,
            GroupId = announcement.GroupId,
            PublishedAtUtc = announcement.PublishedAtUtc,
            CreatedByUserId = announcement.CreatedByUserId,
            CreatedAtUtc = announcement.CreatedAtUtc
        };
}
