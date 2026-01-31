namespace Academy.Application.Contracts.Programs;

public sealed class ProgramDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
