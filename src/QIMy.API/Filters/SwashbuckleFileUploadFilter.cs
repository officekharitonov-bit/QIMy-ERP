using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QIMy.API.Filters;

/// <summary>
/// Swagger filter to handle file uploads properly
/// </summary>
public class SwashbuckleFileUploadFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null || operation.Parameters.Count == 0)
            return;

        // Find form parameters that have IFormFile
        var formFileParams = context.ApiDescription.ActionDescriptor.Parameters
            .Where(p => p.ParameterType == typeof(IFormFile) ||
                       p.ParameterType == typeof(List<IFormFile>))
            .ToList();

        if (!formFileParams.Any())
            return;

        foreach (var formFileParam in formFileParams)
        {
            var paramName = formFileParam.Name;
            var param = operation.Parameters.FirstOrDefault(p => p.Name == paramName);
            
            if (param != null)
            {
                param.Schema = new OpenApiSchema()
                {
                    Type = "string",
                    Format = "binary"
                };
            }
        }
    }
}
