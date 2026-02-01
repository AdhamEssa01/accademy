namespace Academy.Domain;

public sealed class CmsSection : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid PageId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string JsonContent { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
