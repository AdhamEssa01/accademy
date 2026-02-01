namespace Academy.Application.Contracts.Notifications;

public sealed class NotificationDto
{
    public Guid Id { get; set; }

    public Guid? AnnouncementId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }
}
