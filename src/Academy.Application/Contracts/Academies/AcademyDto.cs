namespace Academy.Application.Contracts.Academies;

public sealed class AcademyDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
