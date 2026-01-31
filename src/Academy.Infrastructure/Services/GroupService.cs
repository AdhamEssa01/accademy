using Academy.Application.Abstractions.Scheduling;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Groups;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class GroupService : IGroupService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public GroupService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<GroupDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Groups
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                AcademyId = g.AcademyId,
                ProgramId = g.ProgramId,
                CourseId = g.CourseId,
                LevelId = g.LevelId,
                Name = g.Name,
                InstructorUserId = g.InstructorUserId,
                CreatedAtUtc = g.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<GroupDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var group = await _dbContext.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null)
        {
            throw new NotFoundException();
        }

        return Map(group);
    }

    public async Task<GroupDto> CreateAsync(CreateGroupRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var programExists = await _dbContext.Programs
            .AnyAsync(p => p.Id == request.ProgramId, ct);
        if (!programExists)
        {
            throw new NotFoundException();
        }

        var courseExists = await _dbContext.Courses
            .AnyAsync(c => c.Id == request.CourseId, ct);
        if (!courseExists)
        {
            throw new NotFoundException();
        }

        var levelExists = await _dbContext.Levels
            .AnyAsync(l => l.Id == request.LevelId, ct);
        if (!levelExists)
        {
            throw new NotFoundException();
        }

        var group = new Group
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            ProgramId = request.ProgramId,
            CourseId = request.CourseId,
            LevelId = request.LevelId,
            Name = request.Name,
            InstructorUserId = request.InstructorUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Groups.Add(group);
        await _dbContext.SaveChangesAsync(ct);

        return Map(group);
    }

    public async Task<GroupDto> UpdateAsync(Guid id, UpdateGroupRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null)
        {
            throw new NotFoundException();
        }

        var programExists = await _dbContext.Programs
            .AnyAsync(p => p.Id == request.ProgramId, ct);
        if (!programExists)
        {
            throw new NotFoundException();
        }

        var courseExists = await _dbContext.Courses
            .AnyAsync(c => c.Id == request.CourseId, ct);
        if (!courseExists)
        {
            throw new NotFoundException();
        }

        var levelExists = await _dbContext.Levels
            .AnyAsync(l => l.Id == request.LevelId, ct);
        if (!levelExists)
        {
            throw new NotFoundException();
        }

        group.ProgramId = request.ProgramId;
        group.CourseId = request.CourseId;
        group.LevelId = request.LevelId;
        group.Name = request.Name;

        await _dbContext.SaveChangesAsync(ct);

        return Map(group);
    }

    public async Task<GroupDto> AssignInstructorAsync(Guid id, AssignInstructorRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null)
        {
            throw new NotFoundException();
        }

        group.InstructorUserId = request.InstructorUserId;
        await _dbContext.SaveChangesAsync(ct);

        return Map(group);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (group is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<GroupDto>> ListMineAsync(Guid instructorUserId, PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Groups
            .AsNoTracking()
            .Where(g => g.InstructorUserId == instructorUserId)
            .OrderBy(g => g.Name)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                AcademyId = g.AcademyId,
                ProgramId = g.ProgramId,
                CourseId = g.CourseId,
                LevelId = g.LevelId,
                Name = g.Name,
                InstructorUserId = g.InstructorUserId,
                CreatedAtUtc = g.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    private static GroupDto Map(Group group)
        => new()
        {
            Id = group.Id,
            AcademyId = group.AcademyId,
            ProgramId = group.ProgramId,
            CourseId = group.CourseId,
            LevelId = group.LevelId,
            Name = group.Name,
            InstructorUserId = group.InstructorUserId,
            CreatedAtUtc = group.CreatedAtUtc
        };
}
