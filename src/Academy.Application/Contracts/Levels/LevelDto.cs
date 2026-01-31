namespace Academy.Application.Contracts.Levels;

public sealed class LevelDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid CourseId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
