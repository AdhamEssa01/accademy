namespace Academy.Application.Contracts.Demo;

public sealed class CreateDemoRequest
{
    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }
}