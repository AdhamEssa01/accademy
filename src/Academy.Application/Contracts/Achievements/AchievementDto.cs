namespace Academy.Application.Contracts.Achievements;

public sealed class AchievementDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DateUtc { get; set; }

    public string? MediaUrl { get; set; }

    public string? Tags { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
