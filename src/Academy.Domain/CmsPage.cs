namespace Academy.Domain;

public sealed class CmsPage : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? Title { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
