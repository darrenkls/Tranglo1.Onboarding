using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.Infrastructure.Swagger
{
    public class FileOperationFilter : IOperationFilter
    { 
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
           
            var isFileUploadOperation =
                context.MethodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(FileParameterAttribute));
            if (!isFileUploadOperation) return;

            var uploadFileMediaType = new OpenApiMediaType()
            {
                Schema = new OpenApiSchema()
                {
                    Type = "object",
                    Properties =
                    {
                        ["uploadedFile"] = new OpenApiSchema()
                        {
                            Description = "Upload File",
                            Type = "file",
                            Format = "binary",
                            
                        }
                    },
                    Required = new HashSet<string>()
                    {
                        "uploadedFile"
                    }
                }
            };
            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = uploadFileMediaType
                }
            };

         
        }
    }
}
