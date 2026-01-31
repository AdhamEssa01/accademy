namespace Academy.Application.Contracts.Students;

public sealed class StudentDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
