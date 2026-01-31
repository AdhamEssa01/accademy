namespace Academy.Application.Contracts.Branches;

public sealed class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
}
