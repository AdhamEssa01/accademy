using Microsoft.AspNetCore.Http;

namespace Academy.Api.Models;

public sealed class FileUploadRequest
{
    public IFormFile File { get; set; } = null!;
}
