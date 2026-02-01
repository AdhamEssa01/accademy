using Academy.Application.Abstractions.Achievements;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Achievements;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AchievementService : IAchievementService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public AchievementService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<AchievementDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Achievements
            .AsNoTracking()
            .OrderByDescending(a => a.DateUtc)
            .Select(a => new AchievementDto
            {
                Id = a.Id,
                AcademyId = a.AcademyId,
                Title = a.Title,
                Description = a.Description,
                DateUtc = a.DateUtc,
                MediaUrl = a.MediaUrl,
                Tags = a.Tags,
                CreatedAtUtc = a.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<AchievementDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var achievement = await _dbContext.Achievements
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (achievement is null)
        {
            throw new NotFoundException();
        }

        return Map(achievement);
    }

    public async Task<AchievementDto> CreateAsync(CreateAchievementRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var achievement = new Achievement
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            Title = request.Title,
            Description = request.Description,
            DateUtc = request.DateUtc,
            MediaUrl = request.MediaUrl,
            Tags = request.Tags,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Achievements.Add(achievement);
        await _dbContext.SaveChangesAsync(ct);

        return Map(achievement);
    }

    public async Task<AchievementDto> UpdateAsync(Guid id, UpdateAchievementRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var achievement = await _dbContext.Achievements
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (achievement is null)
        {
            throw new NotFoundException();
        }

        achievement.Title = request.Title;
        achievement.Description = request.Description;
        achievement.DateUtc = request.DateUtc;
        achievement.MediaUrl = request.MediaUrl;
        achievement.Tags = request.Tags;

        await _dbContext.SaveChangesAsync(ct);

        return Map(achievement);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var achievement = await _dbContext.Achievements
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (achievement is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Achievements.Remove(achievement);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<AchievementDto>> ListPublicAsync(PagedRequest request, CancellationToken ct)
    {
        var query = _dbContext.Achievements
            .AsNoTracking()
            .OrderByDescending(a => a.DateUtc)
            .Select(a => new AchievementDto
            {
                Id = a.Id,
                AcademyId = a.AcademyId,
                Title = a.Title,
                Description = a.Description,
                DateUtc = a.DateUtc,
                MediaUrl = a.MediaUrl,
                Tags = a.Tags,
                CreatedAtUtc = a.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    private static AchievementDto Map(Achievement achievement)
        => new()
        {
            Id = achievement.Id,
            AcademyId = achievement.AcademyId,
            Title = achievement.Title,
            Description = achievement.Description,
            DateUtc = achievement.DateUtc,
            MediaUrl = achievement.MediaUrl,
            Tags = achievement.Tags,
            CreatedAtUtc = achievement.CreatedAtUtc
        };
}
