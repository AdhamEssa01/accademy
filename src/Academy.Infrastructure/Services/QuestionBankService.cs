using Academy.Application.Abstractions.Questions;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Questions;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class QuestionBankService : IQuestionBankService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public QuestionBankService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResponse<QuestionDto>> ListAsync(
        Guid? programId,
        Guid? courseId,
        Guid? levelId,
        QuestionType? type,
        QuestionDifficulty? difficulty,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Questions.AsNoTracking();

        if (programId.HasValue)
        {
            query = query.Where(q => q.ProgramId == programId.Value);
        }

        if (courseId.HasValue)
        {
            query = query.Where(q => q.CourseId == courseId.Value);
        }

        if (levelId.HasValue)
        {
            query = query.Where(q => q.LevelId == levelId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(q => q.Type == type.Value);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(q => q.Difficulty == difficulty.Value);
        }

        var page = await query
            .OrderByDescending(q => q.CreatedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        return await MapPagedAsync(page, ct);
    }

    public async Task<QuestionDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var question = await _dbContext.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        if (question is null)
        {
            throw new NotFoundException();
        }

        var options = await _dbContext.QuestionOptions
            .AsNoTracking()
            .Where(o => o.QuestionId == question.Id)
            .OrderBy(o => o.SortOrder)
            .Select(o => new QuestionOptionDto
            {
                Id = o.Id,
                Text = o.Text,
                IsCorrect = o.IsCorrect,
                SortOrder = o.SortOrder
            })
            .ToListAsync(ct);

        return Map(question, options);
    }

    public async Task<QuestionDto> CreateAsync(CreateQuestionRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        await EnsureScopeReferencesAsync(request.ProgramId, request.CourseId, request.LevelId, ct);

        var question = new Question
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            ProgramId = request.ProgramId,
            CourseId = request.CourseId,
            LevelId = request.LevelId,
            Type = request.Type,
            Text = request.Text,
            Difficulty = request.Difficulty,
            Tags = request.Tags,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        var options = BuildOptions(request.Type, request.Options, question.Id, academyId);

        _dbContext.Questions.Add(question);
        if (options.Count > 0)
        {
            _dbContext.QuestionOptions.AddRange(options);
        }

        await _dbContext.SaveChangesAsync(ct);

        return Map(question, options.Select(MapOption).ToList());
    }

    public async Task<QuestionDto> UpdateAsync(Guid id, UpdateQuestionRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var question = await _dbContext.Questions
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        if (question is null)
        {
            throw new NotFoundException();
        }

        await EnsureScopeReferencesAsync(request.ProgramId, request.CourseId, request.LevelId, ct);

        question.ProgramId = request.ProgramId;
        question.CourseId = request.CourseId;
        question.LevelId = request.LevelId;
        question.Type = request.Type;
        question.Text = request.Text;
        question.Difficulty = request.Difficulty;
        question.Tags = request.Tags;

        var existingOptions = await _dbContext.QuestionOptions
            .Where(o => o.QuestionId == question.Id)
            .ToListAsync(ct);

        if (existingOptions.Count > 0)
        {
            _dbContext.QuestionOptions.RemoveRange(existingOptions);
        }

        var options = BuildOptions(request.Type, request.Options, question.Id, question.AcademyId);
        if (options.Count > 0)
        {
            _dbContext.QuestionOptions.AddRange(options);
        }

        await _dbContext.SaveChangesAsync(ct);

        return Map(question, options.Select(MapOption).ToList());
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var question = await _dbContext.Questions
            .FirstOrDefaultAsync(q => q.Id == id, ct);

        if (question is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Questions.Remove(question);
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

    private static List<QuestionOption> BuildOptions(
        QuestionType type,
        IReadOnlyCollection<CreateQuestionOptionRequest> options,
        Guid questionId,
        Guid academyId)
    {
        if (type != QuestionType.MCQ && type != QuestionType.TrueFalse)
        {
            return new List<QuestionOption>();
        }

        if (options.Count == 0)
        {
            throw new ArgumentException("Options are required for MCQ/TrueFalse questions.");
        }

        if (!options.Any(o => o.IsCorrect))
        {
            throw new ArgumentException("At least one correct option is required.");
        }

        return options.Select(option => new QuestionOption
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            QuestionId = questionId,
            Text = option.Text,
            IsCorrect = option.IsCorrect,
            SortOrder = option.SortOrder
        }).ToList();
    }

    private async Task<PagedResponse<QuestionDto>> MapPagedAsync(
        PagedResponse<Question> page,
        CancellationToken ct)
    {
        if (page.Items.Count == 0)
        {
            return new PagedResponse<QuestionDto>(Array.Empty<QuestionDto>(), page.Page, page.PageSize, page.Total);
        }

        var questionIds = page.Items.Select(q => q.Id).ToArray();

        var options = await _dbContext.QuestionOptions
            .AsNoTracking()
            .Where(o => questionIds.Contains(o.QuestionId))
            .OrderBy(o => o.SortOrder)
            .Select(o => new
            {
                o.QuestionId,
                Option = new QuestionOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    SortOrder = o.SortOrder
                }
            })
            .ToListAsync(ct);

        var optionsLookup = options
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Option).ToList());

        var mapped = page.Items.Select(question =>
        {
            var questionOptions = optionsLookup.TryGetValue(question.Id, out var list)
                ? list
                : new List<QuestionOptionDto>();

            return Map(question, questionOptions);
        }).ToList();

        return new PagedResponse<QuestionDto>(mapped, page.Page, page.PageSize, page.Total);
    }

    private static QuestionDto Map(Question question, IReadOnlyList<QuestionOptionDto> options)
        => new()
        {
            Id = question.Id,
            AcademyId = question.AcademyId,
            ProgramId = question.ProgramId,
            CourseId = question.CourseId,
            LevelId = question.LevelId,
            Type = question.Type,
            Text = question.Text,
            Difficulty = question.Difficulty,
            Tags = question.Tags,
            CreatedByUserId = question.CreatedByUserId,
            CreatedAtUtc = question.CreatedAtUtc,
            Options = options
        };

    private static QuestionOptionDto MapOption(QuestionOption option)
        => new()
        {
            Id = option.Id,
            Text = option.Text,
            IsCorrect = option.IsCorrect,
            SortOrder = option.SortOrder
        };
}
