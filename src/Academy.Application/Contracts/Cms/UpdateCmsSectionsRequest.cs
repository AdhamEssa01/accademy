namespace Academy.Application.Contracts.Cms;

public sealed class UpdateCmsSectionsRequest
{
    public List<CmsSectionUpsertRequest> Sections { get; set; } = new();
}
