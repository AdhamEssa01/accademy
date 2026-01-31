namespace Academy.Domain;

public sealed class Branch : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
