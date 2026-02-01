namespace Academy.Application.Contracts.Assignments;

public sealed class AssignmentDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid GroupId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsGroupWide { get; set; }

    public IReadOnlyList<Guid> TargetStudentIds { get; set; } = Array.Empty<Guid>();

    public IReadOnlyList<AssignmentAttachmentDto> Attachments { get; set; } = Array.Empty<AssignmentAttachmentDto>();
}
