using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Text.Json;

namespace Tranglo1.Onboarding.Application.Infrastructure.Swagger
{
	/// <summary>
	/// Source: 
	/// https://github.com/Microsoft/aspnet-api-versioning/wiki/Swashbuckle-Integration
	///	https://github.com/microsoft/aspnet-api-versioning/blob/master/samples/aspnetcore/SwaggerODataSample/SwaggerDefaultValues.cs
	/// </summary>
	internal class SwaggerDefaultValues : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
            var apiDescription = context.ApiDescription;

            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
            {
                return;
            }

			var _ApiVersionParam = operation.Parameters
				.FirstOrDefault(p => string.Equals(p.Name, "version", StringComparison.OrdinalIgnoreCase));
			if (_ApiVersionParam != null)
			{
				operation.Parameters.Remove(_ApiVersionParam);
			}


			//Note: Do not manipulate ApiDescription here as IOperationFilter is all about
			// swagger operations, not "Api explorer" related.

			//if (apiDescription.RelativePath.Contains("v{version}", StringComparison.OrdinalIgnoreCase))
			//{
			//	apiDescription.RelativePath = 
			//		apiDescription.RelativePath.Replace("v{version}", context.DocumentName);
			//}



			//foreach (var parameter in operation.Parameters)
			//{
			//	var description = apiDescription.ParameterDescriptions
			//									.First(p => p.Name == parameter.Name);

			//	if (parameter.Description == null)
			//	{
			//		parameter.Description = description.ModelMetadata?.Description;
			//	}

			//	if (parameter.Schema.Default == null && description.DefaultValue != null)
			//	{
			//		var json = JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType);
			//		parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
			//	}

			//	parameter.Required |= description.IsRequired;
			//}
		}
	}
}
