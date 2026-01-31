using Academy.Application.Abstractions.Security;
using Academy.Application.Abstractions.Students;
using Academy.Application.Contracts.Students;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class StudentService : IStudentService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public StudentService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<StudentDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var query = _dbContext.Students
            .AsNoTracking()
            .OrderBy(s => s.FullName)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                AcademyId = s.AcademyId,
                FullName = s.FullName,
                DateOfBirth = s.DateOfBirth,
                PhotoUrl = s.PhotoUrl,
                Notes = s.Notes,
                CreatedAtUtc = s.CreatedAtUtc
            });

        return await query.ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<StudentDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var student = await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (student is null)
        {
            throw new NotFoundException();
        }

        return Map(student);
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var student = new Student
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync(ct);

        return Map(student);
    }

    public async Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (student is null)
        {
            throw new NotFoundException();
        }

        student.FullName = request.FullName;
        student.DateOfBirth = request.DateOfBirth;
        student.Notes = request.Notes;

        await _dbContext.SaveChangesAsync(ct);

        return Map(student);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (student is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync(ct);
    }

    private static StudentDto Map(Student student)
        => new()
        {
            Id = student.Id,
            AcademyId = student.AcademyId,
            FullName = student.FullName,
            DateOfBirth = student.DateOfBirth,
            PhotoUrl = student.PhotoUrl,
            Notes = student.Notes,
            CreatedAtUtc = student.CreatedAtUtc
        };
}
