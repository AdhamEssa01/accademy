using Academy.Application.Abstractions.Parents;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Students;
using Academy.Application.Exceptions;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ParentPortalService : IParentPortalService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;

    public ParentPortalService(AppDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyList<StudentDto>> GetMyChildrenAsync(CancellationToken ct)
    {
        if (_currentUserContext.UserId is null)
        {
            throw new ForbiddenException();
        }

        var guardianId = await _dbContext.Guardians
            .AsNoTracking()
            .Where(g => g.UserId == _currentUserContext.UserId)
            .Select(g => g.Id)
            .FirstOrDefaultAsync(ct);

        if (guardianId == Guid.Empty)
        {
            return Array.Empty<StudentDto>();
        }

        var query = from link in _dbContext.StudentGuardians.AsNoTracking()
                    join student in _dbContext.Students.AsNoTracking()
                        on link.StudentId equals student.Id
                    where link.GuardianId == guardianId
                    orderby student.FullName
                    select new StudentDto
                    {
                        Id = student.Id,
                        AcademyId = student.AcademyId,
                        FullName = student.FullName,
                        DateOfBirth = student.DateOfBirth,
                        PhotoUrl = student.PhotoUrl,
                        Notes = student.Notes,
                        CreatedAtUtc = student.CreatedAtUtc
                    };

        return await query.ToListAsync(ct);
    }
}
