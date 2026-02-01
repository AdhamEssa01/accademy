namespace Academy.Application.Contracts.Cms;

public sealed class CmsPageDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? Title { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public IReadOnlyList<CmsSectionDto> Sections { get; set; } = Array.Empty<CmsSectionDto>();
}
