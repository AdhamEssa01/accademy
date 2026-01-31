namespace Academy.Application.Contracts.Courses;

public sealed class CourseDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid ProgramId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
