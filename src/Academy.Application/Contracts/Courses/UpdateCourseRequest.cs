namespace Academy.Application.Contracts.Courses;

public sealed class UpdateCourseRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
