namespace Academy.Application.Contracts.Programs;

public sealed class UpdateProgramRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
