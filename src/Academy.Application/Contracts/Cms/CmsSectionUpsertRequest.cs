namespace Academy.Application.Contracts.Cms;

public sealed class CmsSectionUpsertRequest
{
    public string Type { get; set; } = string.Empty;

    public string JsonContent { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; }
}
