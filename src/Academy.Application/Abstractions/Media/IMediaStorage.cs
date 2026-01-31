namespace Academy.Application.Abstractions.Media;

public interface IMediaStorage
{
    Task<string> SaveAsync(
        Stream content,
        string contentType,
        string fileName,
        string folder,
        CancellationToken ct);
}
