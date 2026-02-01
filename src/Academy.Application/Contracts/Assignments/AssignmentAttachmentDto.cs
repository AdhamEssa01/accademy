namespace Academy.Application.Contracts.Assignments;

public sealed class AssignmentAttachmentDto
{
    public Guid Id { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
