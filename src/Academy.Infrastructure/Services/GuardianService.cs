using Academy.Application.Abstractions.Guardians;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Guardians;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class GuardianService : IGuardianService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public GuardianService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<GuardianDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Guardians
            .AsNoTracking()
            .OrderBy(g => g.FullName)
            .Select(g => new GuardianDto
            {
                Id = g.Id,
                AcademyId = g.AcademyId,
                FullName = g.FullName,
                Phone = g.Phone,
                Email = g.Email,
                UserId = g.UserId,
                CreatedAtUtc = g.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<GuardianDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var guardian = await _dbContext.Guardians
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (guardian is null)
        {
            throw new NotFoundException();
        }

        return Map(guardian);
    }

    public async Task<GuardianDto> CreateAsync(CreateGuardianRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var guardian = new Guardian
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Guardians.Add(guardian);
        await _dbContext.SaveChangesAsync(ct);

        return Map(guardian);
    }

    public async Task<GuardianDto> UpdateAsync(Guid id, UpdateGuardianRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var guardian = await _dbContext.Guardians
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (guardian is null)
        {
            throw new NotFoundException();
        }

        guardian.FullName = request.FullName;
        guardian.Phone = request.Phone;
        guardian.Email = request.Email;

        await _dbContext.SaveChangesAsync(ct);

        return Map(guardian);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var guardian = await _dbContext.Guardians
            .FirstOrDefaultAsync(g => g.Id == id, ct);

        if (guardian is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Guardians.Remove(guardian);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task LinkGuardianToStudentAsync(
        Guid guardianId,
        Guid studentId,
        LinkGuardianToStudentRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var guardian = await _dbContext.Guardians
            .FirstOrDefaultAsync(g => g.Id == guardianId, ct);

        if (guardian is null)
        {
            throw new NotFoundException();
        }

        var studentExists = await _dbContext.Students
            .AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists)
        {
            throw new NotFoundException();
        }

        var link = await _dbContext.StudentGuardians
            .FirstOrDefaultAsync(sg => sg.GuardianId == guardianId && sg.StudentId == studentId, ct);

        if (link is null)
        {
            link = new StudentGuardian
            {
                Id = Guid.NewGuid(),
                AcademyId = guardian.AcademyId,
                GuardianId = guardianId,
                StudentId = studentId,
                Relation = request.Relation,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.StudentGuardians.Add(link);
        }
        else
        {
            link.Relation = request.Relation;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task LinkGuardianToUserAsync(Guid guardianId, LinkGuardianToUserRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var guardian = await _dbContext.Guardians
            .FirstOrDefaultAsync(g => g.Id == guardianId, ct);

        if (guardian is null)
        {
            throw new NotFoundException();
        }

        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == request.UserId, ct);
        if (!userExists)
        {
            throw new NotFoundException();
        }

        guardian.UserId = request.UserId;
        await _dbContext.SaveChangesAsync(ct);
    }

    private static GuardianDto Map(Guardian guardian)
        => new()
        {
            Id = guardian.Id,
            AcademyId = guardian.AcademyId,
            FullName = guardian.FullName,
            Phone = guardian.Phone,
            Email = guardian.Email,
            UserId = guardian.UserId,
            CreatedAtUtc = guardian.CreatedAtUtc
        };
}
