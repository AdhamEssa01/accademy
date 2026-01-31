using Academy.Application.Abstractions.Academy;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Academies;
using Academy.Application.Exceptions;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AcademyService : IAcademyService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public AcademyService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<AcademyDto> GetMyAcademyAsync(CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var academy = await _dbContext.Academies
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == academyId, ct);

        if (academy is null)
        {
            throw new NotFoundException();
        }

        return new AcademyDto
        {
            Id = academy.Id,
            Name = academy.Name,
            CreatedAtUtc = academy.CreatedAtUtc
        };
    }

    public async Task<AcademyDto> UpdateMyAcademyAsync(UpdateAcademyRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var academy = await _dbContext.Academies
            .FirstOrDefaultAsync(a => a.Id == academyId, ct);

        if (academy is null)
        {
            throw new NotFoundException();
        }

        academy.Name = request.Name;
        await _dbContext.SaveChangesAsync(ct);

        return new AcademyDto
        {
            Id = academy.Id,
            Name = academy.Name,
            CreatedAtUtc = academy.CreatedAtUtc
        };
    }
}
