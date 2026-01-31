namespace Academy.Domain;

public class UserProfile : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid AcademyId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}
