namespace Academy.Application.Contracts.Groups;

public sealed class CreateGroupRequest
{
    public Guid ProgramId { get; set; }

    public Guid CourseId { get; set; }

    public Guid LevelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? InstructorUserId { get; set; }
}
