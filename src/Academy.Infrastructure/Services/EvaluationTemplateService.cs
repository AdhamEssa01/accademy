using Academy.Application.Abstractions.Evaluations;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Evaluations;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class EvaluationTemplateService : IEvaluationTemplateService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public EvaluationTemplateService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<EvaluationTemplateDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.EvaluationTemplates
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new EvaluationTemplateDto
            {
                Id = t.Id,
                AcademyId = t.AcademyId,
                ProgramId = t.ProgramId,
                CourseId = t.CourseId,
                LevelId = t.LevelId,
                Name = t.Name,
                Description = t.Description,
                CreatedAtUtc = t.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<EvaluationTemplateDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var template = await _dbContext.EvaluationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (template is null)
        {
            throw new NotFoundException();
        }

        return Map(template);
    }

    public async Task<EvaluationTemplateDto> CreateAsync(CreateEvaluationTemplateRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        await EnsureScopeReferencesAsync(request.ProgramId, request.CourseId, request.LevelId, ct);

        var template = new EvaluationTemplate
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            ProgramId = request.ProgramId,
            CourseId = request.CourseId,
            LevelId = request.LevelId,
            Name = request.Name,
            Description = request.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.EvaluationTemplates.Add(template);
        await _dbContext.SaveChangesAsync(ct);

        return Map(template);
    }

    public async Task<EvaluationTemplateDto> UpdateAsync(Guid id, UpdateEvaluationTemplateRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var template = await _dbContext.EvaluationTemplates
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (template is null)
        {
            throw new NotFoundException();
        }

        await EnsureScopeReferencesAsync(request.ProgramId, request.CourseId, request.LevelId, ct);

        template.ProgramId = request.ProgramId;
        template.CourseId = request.CourseId;
        template.LevelId = request.LevelId;
        template.Name = request.Name;
        template.Description = request.Description;

        await _dbContext.SaveChangesAsync(ct);

        return Map(template);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var template = await _dbContext.EvaluationTemplates
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (template is null)
        {
            throw new NotFoundException();
        }

        _dbContext.EvaluationTemplates.Remove(template);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<RubricCriterionDto>> ListCriteriaAsync(
        Guid templateId,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var templateExists = await _dbContext.EvaluationTemplates
            .AnyAsync(t => t.Id == templateId, ct);
        if (!templateExists)
        {
            throw new NotFoundException();
        }

        var query = _dbContext.RubricCriteria
            .AsNoTracking()
            .Where(c => c.TemplateId == templateId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new RubricCriterionDto
            {
                Id = c.Id,
                TemplateId = c.TemplateId,
                Name = c.Name,
                MaxScore = c.MaxScore,
                Weight = c.Weight,
                SortOrder = c.SortOrder,
                CreatedAtUtc = c.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<RubricCriterionDto> CreateCriterionAsync(
        Guid templateId,
        CreateRubricCriterionRequest request,
        CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var templateExists = await _dbContext.EvaluationTemplates
            .AnyAsync(t => t.Id == templateId, ct);
        if (!templateExists)
        {
            throw new NotFoundException();
        }

        var criterion = new RubricCriterion
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            TemplateId = templateId,
            Name = request.Name,
            MaxScore = request.MaxScore,
            Weight = request.Weight,
            SortOrder = request.SortOrder,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.RubricCriteria.Add(criterion);
        await _dbContext.SaveChangesAsync(ct);

        return Map(criterion);
    }

    public async Task<RubricCriterionDto> UpdateCriterionAsync(
        Guid templateId,
        Guid criterionId,
        UpdateRubricCriterionRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var criterion = await _dbContext.RubricCriteria
            .FirstOrDefaultAsync(c => c.Id == criterionId && c.TemplateId == templateId, ct);

        if (criterion is null)
        {
            throw new NotFoundException();
        }

        criterion.Name = request.Name;
        criterion.MaxScore = request.MaxScore;
        criterion.Weight = request.Weight;
        criterion.SortOrder = request.SortOrder;

        await _dbContext.SaveChangesAsync(ct);

        return Map(criterion);
    }

    public async Task DeleteCriterionAsync(Guid templateId, Guid criterionId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var criterion = await _dbContext.RubricCriteria
            .FirstOrDefaultAsync(c => c.Id == criterionId && c.TemplateId == templateId, ct);

        if (criterion is null)
        {
            throw new NotFoundException();
        }

        _dbContext.RubricCriteria.Remove(criterion);
        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task EnsureScopeReferencesAsync(
        Guid? programId,
        Guid? courseId,
        Guid? levelId,
        CancellationToken ct)
    {
        if (programId.HasValue)
        {
            var exists = await _dbContext.Programs.AnyAsync(p => p.Id == programId.Value, ct);
            if (!exists)
            {
                throw new NotFoundException();
            }
        }

        if (courseId.HasValue)
        {
            var exists = await _dbContext.Courses.AnyAsync(c => c.Id == courseId.Value, ct);
            if (!exists)
            {
                throw new NotFoundException();
            }
        }

        if (levelId.HasValue)
        {
            var exists = await _dbContext.Levels.AnyAsync(l => l.Id == levelId.Value, ct);
            if (!exists)
            {
                throw new NotFoundException();
            }
        }
    }

    private static EvaluationTemplateDto Map(EvaluationTemplate template)
        => new()
        {
            Id = template.Id,
            AcademyId = template.AcademyId,
            ProgramId = template.ProgramId,
            CourseId = template.CourseId,
            LevelId = template.LevelId,
            Name = template.Name,
            Description = template.Description,
            CreatedAtUtc = template.CreatedAtUtc
        };

    private static RubricCriterionDto Map(RubricCriterion criterion)
        => new()
        {
            Id = criterion.Id,
            TemplateId = criterion.TemplateId,
            Name = criterion.Name,
            MaxScore = criterion.MaxScore,
            Weight = criterion.Weight,
            SortOrder = criterion.SortOrder,
            CreatedAtUtc = criterion.CreatedAtUtc
        };
}
