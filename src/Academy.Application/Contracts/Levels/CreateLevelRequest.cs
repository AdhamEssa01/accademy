namespace Academy.Application.Contracts.Levels;

public sealed class CreateLevelRequest
{
    public Guid CourseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
