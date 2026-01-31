using Academy.Application.Abstractions.Media;

namespace Academy.Api.Storage;

public sealed class LocalMediaStorage : IMediaStorage
{
    private readonly IWebHostEnvironment _environment;

    public LocalMediaStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string contentType,
        string fileName,
        string folder,
        CancellationToken ct)
    {
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var safeFileName = Path.GetFileName(fileName);
        var safeFolder = folder.Trim('/').Replace('/', Path.DirectorySeparatorChar);
        var targetFolder = Path.Combine(webRoot, safeFolder);

        Directory.CreateDirectory(targetFolder);

        var filePath = Path.Combine(targetFolder, safeFileName);
        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);
        await content.CopyToAsync(fileStream, ct);

        var relativePath = "/" + folder.Trim('/').Replace('\\', '/') + "/" + safeFileName;
        return relativePath;
    }
}
