using Academy.Domain;

namespace Academy.Application.Contracts.Announcements;

public sealed class AnnouncementDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public AnnouncementAudience Audience { get; set; }

    public Guid? GroupId { get; set; }

    public DateTime PublishedAtUtc { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
