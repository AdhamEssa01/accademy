using Academy.Domain;

namespace Academy.Application.Contracts.Announcements;

public sealed class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public AnnouncementAudience Audience { get; set; }

    public Guid? GroupId { get; set; }
}
