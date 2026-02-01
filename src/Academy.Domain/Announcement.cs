namespace Academy.Domain;

public sealed class Announcement : IAcademyScoped
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
