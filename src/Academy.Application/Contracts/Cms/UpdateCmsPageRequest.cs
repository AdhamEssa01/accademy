namespace Academy.Application.Contracts.Cms;

public sealed class UpdateCmsPageRequest
{
    public string? Title { get; set; }

    public DateTime? PublishedAtUtc { get; set; }
}
