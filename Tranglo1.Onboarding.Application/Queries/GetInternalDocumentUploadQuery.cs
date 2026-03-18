using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;
using Dapper;
using Tranglo1.Onboarding.Infrastructure.Services;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    [Permission(Permission.KYCManagementDocumentation.Action_InternalDocument_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class GetInternalDocumentUploadQuery : BaseQuery<Result<List<InternalDocumentUploadOutputDTO>>>
    {
        public int BusinessProfileCode { get; set; }
        public UserType UserType { get; set; }

        public class GetInternalDocumentUploadQueryHandler : IRequestHandler<GetInternalDocumentUploadQuery, Result<List<InternalDocumentUploadOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetInternalDocumentUploadQueryHandler> _logger;

            public GetInternalDocumentUploadQueryHandler(IConfiguration config, ILogger<GetInternalDocumentUploadQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<List<InternalDocumentUploadOutputDTO>>> Handle(GetInternalDocumentUploadQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    bool? isDisplay = request.UserType == UserType.External ? true : (bool?)null;
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetInternalDocumentUpload",
                           new
                           {
                               BusinessProfileCode = request.BusinessProfileCode,
                               isDisplay = isDisplay
                           },
                           null, null, CommandType.StoredProcedure); ;
                        var result = (List<InternalDocumentUploadOutputDTO>)await reader.ReadAsync<InternalDocumentUploadOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetInternalDocumentUploadQuery] {ex.Message}");
                }
                return Result.Failure<List<InternalDocumentUploadOutputDTO>>(
                            $"Get Internal Document Upload failed for {request.BusinessProfileCode}."
                        );
            }
        }

    }
}
