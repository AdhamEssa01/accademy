namespace Academy.Application.Contracts.Guardians;

public sealed class CreateGuardianRequest
{
    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }
}
