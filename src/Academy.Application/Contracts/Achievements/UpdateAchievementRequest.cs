namespace Academy.Application.Contracts.Achievements;

public sealed class UpdateAchievementRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DateUtc { get; set; }

    public string? MediaUrl { get; set; }

    public string? Tags { get; set; }
}
