using Academy.Application.Contracts.Demo;
using Academy.Shared.Pagination;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/demo")]
public sealed class DemoController : ControllerBase
{
    [HttpPost("echo")]
    public ActionResult<CreateDemoRequest> Echo([FromBody] CreateDemoRequest request)
    {
        return Ok(request);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResponse<string>>> Paged(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {
        var data = Enumerable.Range(1, 50)
            .Select(i => $"Item {i}")
            .AsQueryable();

        var response = await data.ToPagedResponseAsync(request.Page, request.PageSize, ct);
        return Ok(response);
    }

    [HttpGet("throw")]
    public IActionResult Throw()
    {
        throw new InvalidOperationException("Demo exception.");
    }
}
