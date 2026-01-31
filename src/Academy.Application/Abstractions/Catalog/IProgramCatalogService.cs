using Academy.Application.Contracts.Courses;
using Academy.Application.Contracts.Levels;
using Academy.Application.Contracts.Programs;
using Academy.Shared.Pagination;

namespace Academy.Application.Abstractions.Catalog;

public interface IProgramCatalogService
{
    Task<PagedResponse<ProgramDto>> ListProgramsAsync(PagedRequest request, CancellationToken ct);

    Task<ProgramDto> GetProgramAsync(Guid id, CancellationToken ct);

    Task<ProgramDto> CreateProgramAsync(CreateProgramRequest request, CancellationToken ct);

    Task<ProgramDto> UpdateProgramAsync(Guid id, UpdateProgramRequest request, CancellationToken ct);

    Task DeleteProgramAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<CourseDto>> ListCoursesAsync(Guid? programId, PagedRequest request, CancellationToken ct);

    Task<CourseDto> GetCourseAsync(Guid id, CancellationToken ct);

    Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct);

    Task<CourseDto> UpdateCourseAsync(Guid id, UpdateCourseRequest request, CancellationToken ct);

    Task DeleteCourseAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<LevelDto>> ListLevelsAsync(Guid? courseId, PagedRequest request, CancellationToken ct);

    Task<LevelDto> GetLevelAsync(Guid id, CancellationToken ct);

    Task<LevelDto> CreateLevelAsync(CreateLevelRequest request, CancellationToken ct);

    Task<LevelDto> UpdateLevelAsync(Guid id, UpdateLevelRequest request, CancellationToken ct);

    Task DeleteLevelAsync(Guid id, CancellationToken ct);
}
