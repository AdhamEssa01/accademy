using Academy.Application.Abstractions.Catalog;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Courses;
using Academy.Application.Contracts.Levels;
using Academy.Application.Contracts.Programs;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ProgramCatalogService : IProgramCatalogService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public ProgramCatalogService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<ProgramDto>> ListProgramsAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Programs
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProgramDto
            {
                Id = p.Id,
                AcademyId = p.AcademyId,
                Name = p.Name,
                Description = p.Description,
                CreatedAtUtc = p.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<ProgramDto> GetProgramAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var program = await _dbContext.Programs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (program is null)
        {
            throw new NotFoundException();
        }

        return MapProgram(program);
    }

    public async Task<ProgramDto> CreateProgramAsync(CreateProgramRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var program = new Academy.Domain.Program
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            Name = request.Name,
            Description = request.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Programs.Add(program);
        await _dbContext.SaveChangesAsync(ct);

        return MapProgram(program);
    }

    public async Task<ProgramDto> UpdateProgramAsync(Guid id, UpdateProgramRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var program = await _dbContext.Programs
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (program is null)
        {
            throw new NotFoundException();
        }

        program.Name = request.Name;
        program.Description = request.Description;
        await _dbContext.SaveChangesAsync(ct);

        return MapProgram(program);
    }

    public async Task DeleteProgramAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var program = await _dbContext.Programs
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (program is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Programs.Remove(program);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<CourseDto>> ListCoursesAsync(Guid? programId, PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Courses.AsNoTracking();
        if (programId.HasValue)
        {
            query = query.Where(c => c.ProgramId == programId.Value);
        }

        var projected = query
            .OrderBy(c => c.Name)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                AcademyId = c.AcademyId,
                ProgramId = c.ProgramId,
                Name = c.Name,
                Description = c.Description,
                CreatedAtUtc = c.CreatedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<CourseDto> GetCourseAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var course = await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course is null)
        {
            throw new NotFoundException();
        }

        return MapCourse(course);
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var programExists = await _dbContext.Programs
            .AnyAsync(p => p.Id == request.ProgramId, ct);

        if (!programExists)
        {
            throw new NotFoundException();
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            ProgramId = request.ProgramId,
            Name = request.Name,
            Description = request.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(ct);

        return MapCourse(course);
    }

    public async Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var course = await _dbContext.Courses
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course is null)
        {
            throw new NotFoundException();
        }

        course.Name = request.Name;
        course.Description = request.Description;
        await _dbContext.SaveChangesAsync(ct);

        return MapCourse(course);
    }

    public async Task DeleteCourseAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var course = await _dbContext.Courses
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Courses.Remove(course);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<LevelDto>> ListLevelsAsync(Guid? courseId, PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Levels.AsNoTracking();
        if (courseId.HasValue)
        {
            query = query.Where(l => l.CourseId == courseId.Value);
        }

        var projected = query
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name)
            .Select(l => new LevelDto
            {
                Id = l.Id,
                AcademyId = l.AcademyId,
                CourseId = l.CourseId,
                Name = l.Name,
                SortOrder = l.SortOrder,
                CreatedAtUtc = l.CreatedAtUtc
            });

        return await projected.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<LevelDto> GetLevelAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var level = await _dbContext.Levels
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (level is null)
        {
            throw new NotFoundException();
        }

        return MapLevel(level);
    }

    public async Task<LevelDto> CreateLevelAsync(CreateLevelRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var courseExists = await _dbContext.Courses
            .AnyAsync(c => c.Id == request.CourseId, ct);

        if (!courseExists)
        {
            throw new NotFoundException();
        }

        var level = new Level
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            CourseId = request.CourseId,
            Name = request.Name,
            SortOrder = request.SortOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Levels.Add(level);
        await _dbContext.SaveChangesAsync(ct);

        return MapLevel(level);
    }

    public async Task<LevelDto> UpdateLevelAsync(Guid id, UpdateLevelRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var level = await _dbContext.Levels
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (level is null)
        {
            throw new NotFoundException();
        }

        level.Name = request.Name;
        level.SortOrder = request.SortOrder;
        await _dbContext.SaveChangesAsync(ct);

        return MapLevel(level);
    }

    public async Task DeleteLevelAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var level = await _dbContext.Levels
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (level is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Levels.Remove(level);
        await _dbContext.SaveChangesAsync(ct);
    }

    private static ProgramDto MapProgram(Academy.Domain.Program program)
        => new()
        {
            Id = program.Id,
            AcademyId = program.AcademyId,
            Name = program.Name,
            Description = program.Description,
            CreatedAtUtc = program.CreatedAtUtc
        };

    private static CourseDto MapCourse(Course course)
        => new()
        {
            Id = course.Id,
            AcademyId = course.AcademyId,
            ProgramId = course.ProgramId,
            Name = course.Name,
            Description = course.Description,
            CreatedAtUtc = course.CreatedAtUtc
        };

    private static LevelDto MapLevel(Level level)
        => new()
        {
            Id = level.Id,
            AcademyId = level.AcademyId,
            CourseId = level.CourseId,
            Name = level.Name,
            SortOrder = level.SortOrder,
            CreatedAtUtc = level.CreatedAtUtc
        };
}
