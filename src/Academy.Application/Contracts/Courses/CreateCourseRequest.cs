namespace Academy.Application.Contracts.Courses;

public sealed class CreateCourseRequest
{
    public Guid ProgramId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
