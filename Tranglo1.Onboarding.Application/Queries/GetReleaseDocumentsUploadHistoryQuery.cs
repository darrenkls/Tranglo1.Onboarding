using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.View)]
    [Permission(Permission.KYCManagementDocumentation.Action_ReleaseDocument_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class GetReleaseDocumentsUploadHistoryQuery : BaseQuery<Result<List<ReleaseDocumentsUploadHistoryOutputDTO>>>
    {
        public int BusinessProfileCode { get; set; }
        public UserType UserType { get; set; }

        public class GetReleaseDocumentsUploadHistoryQueryHandler : IRequestHandler<GetReleaseDocumentsUploadHistoryQuery, Result<List<ReleaseDocumentsUploadHistoryOutputDTO>>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetReleaseDocumentsUploadHistoryQueryHandler> _logger;

            public GetReleaseDocumentsUploadHistoryQueryHandler(IConfiguration config, ILogger<GetReleaseDocumentsUploadHistoryQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<Result<List<ReleaseDocumentsUploadHistoryOutputDTO>>> Handle(GetReleaseDocumentsUploadHistoryQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetReleaseDocumentsUploadHistory",
                           new
                           {
                               BusinessProfileCode = request.BusinessProfileCode,
                           },
                           null, null, CommandType.StoredProcedure); ;
                        var result = (List<ReleaseDocumentsUploadHistoryOutputDTO>)await reader.ReadAsync<ReleaseDocumentsUploadHistoryOutputDTO>();
                        return Result.Success(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetReleaseDocumentsUploadHistoryQuery] {ex.Message}");
                }
                return Result.Failure<List<ReleaseDocumentsUploadHistoryOutputDTO>>(
                            $"Get Release Document Upload History failed for {request.BusinessProfileCode}."
                        );
            }
        }
    }
}
