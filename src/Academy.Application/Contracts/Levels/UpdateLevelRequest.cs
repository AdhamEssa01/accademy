namespace Academy.Application.Contracts.Levels;

public sealed class UpdateLevelRequest
{
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
