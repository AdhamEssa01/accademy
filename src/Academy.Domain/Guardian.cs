namespace Academy.Domain;

public sealed class Guardian : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public Guid? UserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
