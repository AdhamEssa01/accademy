namespace Academy.Application.Contracts.Programs;

public sealed class CreateProgramRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
