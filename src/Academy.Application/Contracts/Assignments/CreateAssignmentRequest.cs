namespace Academy.Application.Contracts.Assignments;

public sealed class CreateAssignmentRequest
{
    public Guid GroupId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public List<Guid>? TargetStudentIds { get; set; }
}
