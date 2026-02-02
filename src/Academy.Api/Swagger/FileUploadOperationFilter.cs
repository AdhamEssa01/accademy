using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Academy.Api.Swagger;

public sealed class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Type == typeof(IFormFile))
            .ToList();

        if (fileParams.Count == 0)
        {
            return;
        }

        operation.Parameters = operation.Parameters
            ?.Where(p => fileParams.All(fp => !string.Equals(fp.Name, p.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>()
        };

        var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var param in fileParams)
        {
            schema.Properties[param.Name] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            };

            if (param.IsRequired)
            {
                required.Add(param.Name);
            }
        }

        if (required.Count > 0)
        {
            schema.Required = required;
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };
    }
}
