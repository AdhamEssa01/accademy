using Academy.Application.Abstractions.Evaluations;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Evaluations;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Academy.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class EvaluationService : IEvaluationService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public EvaluationService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<EvaluationDto> CreateAsync(CreateEvaluationRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        var studentExists = await _dbContext.Students.AnyAsync(s => s.Id == request.StudentId, ct);
        if (!studentExists)
        {
            throw new NotFoundException();
        }

        var templateExists = await _dbContext.EvaluationTemplates.AnyAsync(t => t.Id == request.TemplateId, ct);
        if (!templateExists)
        {
            throw new NotFoundException();
        }

        Session? session = null;
        if (request.SessionId.HasValue)
        {
            session = await _dbContext.Sessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SessionId.Value, ct);

            if (session is null)
            {
                throw new NotFoundException();
            }
        }

        await EnsureInstructorScopeAsync(userId, request.StudentId, session, ct);

        var criterionIds = request.Items
            .Select(i => i.CriterionId)
            .Distinct()
            .ToArray();

        if (criterionIds.Length != request.Items.Count)
        {
            throw new ArgumentException("Duplicate criteria in evaluation items.");
        }

        var criteria = await _dbContext.RubricCriteria
            .AsNoTracking()
            .Where(c => c.TemplateId == request.TemplateId && criterionIds.Contains(c.Id))
            .Select(c => new { c.Id, c.MaxScore, c.Weight })
            .ToListAsync(ct);

        if (criteria.Count != criterionIds.Length)
        {
            throw new NotFoundException();
        }

        var criteriaLookup = criteria.ToDictionary(c => c.Id, c => c);
        var totalScore = 0m;

        foreach (var item in request.Items)
        {
            var criterion = criteriaLookup[item.CriterionId];
            if (item.Score < 0 || item.Score > criterion.MaxScore)
            {
                throw new ArgumentException("Score exceeds max score.");
            }

            totalScore += item.Score * criterion.Weight;
        }

        var evaluation = new Evaluation
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            StudentId = request.StudentId,
            TemplateId = request.TemplateId,
            SessionId = request.SessionId,
            EvaluatedByUserId = userId,
            Notes = request.Notes,
            TotalScore = totalScore,
            CreatedAtUtc = DateTime.UtcNow
        };

        var items = request.Items.Select(item => new EvaluationItem
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            EvaluationId = evaluation.Id,
            CriterionId = item.CriterionId,
            Score = item.Score,
            Comment = item.Comment
        }).ToList();

        _dbContext.Evaluations.Add(evaluation);
        _dbContext.EvaluationItems.AddRange(items);

        await _dbContext.SaveChangesAsync(ct);

        return Map(evaluation, items);
    }

    public async Task<PagedResponse<EvaluationDto>> ListForStudentAsync(
        Guid studentId,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var studentExists = await _dbContext.Students.AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists)
        {
            throw new NotFoundException();
        }

        var page = await _dbContext.Evaluations
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        return await MapPagedAsync(page, ct);
    }

    public async Task<PagedResponse<EvaluationDto>> ParentListMyChildrenAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct)
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
            return new PagedResponse<EvaluationDto>(Array.Empty<EvaluationDto>(), request.Page, request.PageSize, 0);
        }

        var studentIds = await _dbContext.StudentGuardians
            .AsNoTracking()
            .Where(sg => sg.GuardianId == guardianId)
            .Select(sg => sg.StudentId)
            .Distinct()
            .ToListAsync(ct);

        if (studentIds.Count == 0)
        {
            return new PagedResponse<EvaluationDto>(Array.Empty<EvaluationDto>(), request.Page, request.PageSize, 0);
        }

        var query = _dbContext.Evaluations
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId));

        if (from.HasValue)
        {
            var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(e => e.CreatedAtUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(e => e.CreatedAtUtc <= toUtc);
        }

        var page = await query
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        return await MapPagedAsync(page, ct);
    }

    private async Task EnsureInstructorScopeAsync(
        Guid userId,
        Guid studentId,
        Session? session,
        CancellationToken ct)
    {
        var roles = _currentUserContext.Roles;

        if (roles.Contains(Roles.Admin))
        {
            return;
        }

        if (!roles.Contains(Roles.Instructor))
        {
            throw new ForbiddenException();
        }

        if (session is not null && session.InstructorUserId == userId)
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var teachesStudent = await (from enrollment in _dbContext.Enrollments.AsNoTracking()
                                    join grp in _dbContext.Groups.AsNoTracking()
                                        on enrollment.GroupId equals grp.Id
                                    where enrollment.StudentId == studentId
                                        && grp.InstructorUserId == userId
                                        && (enrollment.EndDate == null || enrollment.EndDate >= today)
                                    select enrollment.Id)
            .AnyAsync(ct);

        if (!teachesStudent)
        {
            throw new ForbiddenException();
        }
    }

    private async Task<PagedResponse<EvaluationDto>> MapPagedAsync(
        PagedResponse<Evaluation> page,
        CancellationToken ct)
    {
        if (page.Items.Count == 0)
        {
            return new PagedResponse<EvaluationDto>(Array.Empty<EvaluationDto>(), page.Page, page.PageSize, page.Total);
        }

        var evaluationIds = page.Items.Select(e => e.Id).ToArray();

        var items = await _dbContext.EvaluationItems
            .AsNoTracking()
            .Where(i => evaluationIds.Contains(i.EvaluationId))
            .ToListAsync(ct);

        var itemLookup = items
            .GroupBy(i => i.EvaluationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var mapped = page.Items.Select(evaluation =>
        {
            var evaluationItems = itemLookup.TryGetValue(evaluation.Id, out var list)
                ? list.OrderBy(i => i.CriterionId).Select(Map).ToList()
                : new List<EvaluationItemDto>();

            return new EvaluationDto
            {
                Id = evaluation.Id,
                AcademyId = evaluation.AcademyId,
                StudentId = evaluation.StudentId,
                TemplateId = evaluation.TemplateId,
                SessionId = evaluation.SessionId,
                EvaluatedByUserId = evaluation.EvaluatedByUserId,
                Notes = evaluation.Notes,
                TotalScore = evaluation.TotalScore,
                CreatedAtUtc = evaluation.CreatedAtUtc,
                Items = evaluationItems
            };
        }).ToList();

        return new PagedResponse<EvaluationDto>(mapped, page.Page, page.PageSize, page.Total);
    }

    private static EvaluationDto Map(Evaluation evaluation, IReadOnlyList<EvaluationItem> items)
        => new()
        {
            Id = evaluation.Id,
            AcademyId = evaluation.AcademyId,
            StudentId = evaluation.StudentId,
            TemplateId = evaluation.TemplateId,
            SessionId = evaluation.SessionId,
            EvaluatedByUserId = evaluation.EvaluatedByUserId,
            Notes = evaluation.Notes,
            TotalScore = evaluation.TotalScore,
            CreatedAtUtc = evaluation.CreatedAtUtc,
            Items = items.OrderBy(i => i.CriterionId).Select(Map).ToList()
        };

    private static EvaluationItemDto Map(EvaluationItem item)
        => new()
        {
            Id = item.Id,
            CriterionId = item.CriterionId,
            Score = item.Score,
            Comment = item.Comment
        };
}
