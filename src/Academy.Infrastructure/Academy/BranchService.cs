using Academy.Application.Abstractions.Academy;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Branches;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class BranchService : IBranchService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public BranchService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<BranchDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Branches
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                AcademyId = b.AcademyId,
                Name = b.Name,
                Address = b.Address,
                CreatedAtUtc = b.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<BranchDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (branch is null)
        {
            throw new NotFoundException();
        }

        return Map(branch);
    }

    public async Task<BranchDto> CreateAsync(CreateBranchRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            Name = request.Name,
            Address = request.Address,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync(ct);

        return Map(branch);
    }

    public async Task<BranchDto> UpdateAsync(Guid id, UpdateBranchRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (branch is null)
        {
            throw new NotFoundException();
        }

        branch.Name = request.Name;
        branch.Address = request.Address;
        await _dbContext.SaveChangesAsync(ct);

        return Map(branch);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (branch is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Branches.Remove(branch);
        await _dbContext.SaveChangesAsync(ct);
    }

    private static BranchDto Map(Branch branch)
        => new()
        {
            Id = branch.Id,
            AcademyId = branch.AcademyId,
            Name = branch.Name,
            Address = branch.Address,
            CreatedAtUtc = branch.CreatedAtUtc
        };
}
