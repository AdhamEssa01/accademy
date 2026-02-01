using Academy.Application.Abstractions.Cms;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Cms;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class CmsService : ICmsService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public CmsService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<CmsPageDto> GetPageAsync(string slug, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var page = await _dbContext.CmsPages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (page is null)
        {
            throw new NotFoundException();
        }

        var sectionEntities = await _dbContext.CmsSections
            .AsNoTracking()
            .Where(s => s.PageId == page.Id)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct);

        var sections = sectionEntities.Select(MapSection).ToList();

        return MapPage(page, sections);
    }

    public async Task<CmsPageDto> UpdatePageAsync(string slug, UpdateCmsPageRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var page = await _dbContext.CmsPages
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (page is null)
        {
            page = new CmsPage
            {
                Id = Guid.NewGuid(),
                AcademyId = academyId,
                Slug = slug,
                Title = request.Title,
                PublishedAtUtc = request.PublishedAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.CmsPages.Add(page);
        }
        else
        {
            if (request.Title is not null)
            {
                page.Title = request.Title;
            }

            page.PublishedAtUtc = request.PublishedAtUtc;
        }

        await _dbContext.SaveChangesAsync(ct);

        var sectionEntities = await _dbContext.CmsSections
            .AsNoTracking()
            .Where(s => s.PageId == page.Id)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct);

        var sections = sectionEntities.Select(MapSection).ToList();

        return MapPage(page, sections);
    }

    public async Task<IReadOnlyList<CmsSectionDto>> UpdateSectionsAsync(
        string slug,
        UpdateCmsSectionsRequest request,
        CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var page = await _dbContext.CmsPages
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (page is null)
        {
            page = new CmsPage
            {
                Id = Guid.NewGuid(),
                AcademyId = academyId,
                Slug = slug,
                Title = slug,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.CmsPages.Add(page);
        }

        var existing = await _dbContext.CmsSections
            .Where(s => s.PageId == page.Id)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            _dbContext.CmsSections.RemoveRange(existing);
        }

        var now = DateTime.UtcNow;
        var newSections = request.Sections.Select(section => new CmsSection
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            PageId = page.Id,
            Type = section.Type,
            JsonContent = section.JsonContent,
            SortOrder = section.SortOrder,
            IsVisible = section.IsVisible,
            CreatedAtUtc = now
        }).ToList();

        if (newSections.Count > 0)
        {
            _dbContext.CmsSections.AddRange(newSections);
        }

        await _dbContext.SaveChangesAsync(ct);

        return newSections
            .OrderBy(s => s.SortOrder)
            .Select(MapSection)
            .ToList();
    }

    public async Task<CmsPageDto?> GetPublicPageAsync(string slug, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var page = await _dbContext.CmsPages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.PublishedAtUtc != null && p.PublishedAtUtc <= now, ct);

        if (page is null)
        {
            return null;
        }

        var sectionEntities = await _dbContext.CmsSections
            .AsNoTracking()
            .Where(s => s.PageId == page.Id && s.IsVisible)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct);

        var sections = sectionEntities.Select(MapSection).ToList();

        return MapPage(page, sections);
    }

    private static CmsPageDto MapPage(CmsPage page, IReadOnlyList<CmsSectionDto> sections)
        => new()
        {
            Id = page.Id,
            AcademyId = page.AcademyId,
            Slug = page.Slug,
            Title = page.Title,
            PublishedAtUtc = page.PublishedAtUtc,
            CreatedAtUtc = page.CreatedAtUtc,
            Sections = sections
        };

    private static CmsSectionDto MapSection(CmsSection section)
        => new()
        {
            Id = section.Id,
            PageId = section.PageId,
            Type = section.Type,
            JsonContent = section.JsonContent,
            SortOrder = section.SortOrder,
            IsVisible = section.IsVisible,
            CreatedAtUtc = section.CreatedAtUtc
        };
}
