namespace Academy.Application.Contracts.Guardians;

public sealed class GuardianDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public Guid? UserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
