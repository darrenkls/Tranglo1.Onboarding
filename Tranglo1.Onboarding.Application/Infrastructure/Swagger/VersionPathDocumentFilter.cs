using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.Infrastructure.Swagger
{
	internal class VersionPathDocumentFilter : IDocumentFilter
	{
		public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
		{
			Dictionary<string, string> _OldNewPaths = new Dictionary<string, string>();

			//Note: Do not remove the key while looping the enumerator
			foreach (var path in swaggerDoc.Paths)
			{
				if (path.Key.Contains("v{version}", System.StringComparison.OrdinalIgnoreCase))
				{
					var _NewKey = path.Key.Replace("v{version}", context.DocumentName);
					_OldNewPaths.Add(path.Key, _NewKey);
				}
			}

			foreach (var item in _OldNewPaths)
			{
				var value = swaggerDoc.Paths[item.Key];
				if (swaggerDoc.Paths.Remove(item.Key))
				{
					swaggerDoc.Paths.Add(item.Value, value);
				}
			}
		}
	}
}
