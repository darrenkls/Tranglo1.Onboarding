using AutoMapper;
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
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerAPISetting, UACAction.View)]
    [Permission(Permission.APISettings.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] {})]
    internal class GetPartnerListingSearchResultQuery : BaseQuery<PagedResult<PartnerListingSearchResultOutputDTO>>
    {
        public string PartnerName { get; set; }
        public int? PartnerTypeCode { get; set; }
        public int? SolutionCode { get; set; }
        public int? CurrentAPIEnvironmentCode { get; set; }
        public string EntityCode { get; set; }
        public int? AgreementStatusCode { get; set; }
        public int? PartnerAccountStatusCode { get; set; }
        public string FromRegistrationDate { get; set; }
        public string ToRegistrationDate { get; set; }
        public PagingOptions PagingOptions = new PagingOptions();
        public override Task<string> GetAuditLogAsync(PagedResult<PartnerListingSearchResultOutputDTO> result)
        {
            return Task.FromResult("Searched API partner records");
        }

        public class GetPartnerListingSearchResultQueryHandler : IRequestHandler<GetPartnerListingSearchResultQuery, PagedResult<PartnerListingSearchResultOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly ILogger<GetPartnerListingSearchResultQueryHandler> _logger;


            public GetPartnerListingSearchResultQueryHandler(IConfiguration config, ILogger<GetPartnerListingSearchResultQueryHandler> logger)
            {
                _config = config;
                _logger = logger;
            }

            public async Task<PagedResult<PartnerListingSearchResultOutputDTO>> Handle(GetPartnerListingSearchResultQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    PagedResult<PartnerListingSearchResultOutputDTO> result = new PagedResult<PartnerListingSearchResultOutputDTO>();
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var _sortExpression = string.IsNullOrEmpty(request.PagingOptions.SortExpression) ? "" : request.PagingOptions.SortExpression + " " + (request.PagingOptions.Direction == SortDirection.Ascending ? "" : "DESC");
                        var reader = await connection.QueryMultipleAsync(
                            "dbo.GetPartnerListingsBySearchCriteria",
                            new
                            {
                                PartnerName = request.PartnerName,
                                PartnerTypeCode = request.PartnerTypeCode,
                                SolutionCode = request.SolutionCode,
                                EntityCode = request.EntityCode,
                                CurrentAPIEnvironmentCode = request.CurrentAPIEnvironmentCode,
                                PartnerAgreementStatusCode = request.AgreementStatusCode,
                                PartnerAccountStatusTypeCode = request.PartnerAccountStatusCode,
                                RegistrationDateFrom = request.FromRegistrationDate,
                                RegistrationDateTo = request.ToRegistrationDate,
                                PageIndex = request.PagingOptions.PageIndex,
                                PageSize = request.PagingOptions.PageSize,
                                SortExpression = _sortExpression
                            },
                            null, null,
                             CommandType.StoredProcedure);

                        result.Results = await reader.ReadAsync<PartnerListingSearchResultOutputDTO>();
                        IEnumerable<PaginationInfoDTO> _paginationInfoDTO = await reader.ReadAsync<PaginationInfoDTO>();
                        result.RowCount = _paginationInfoDTO.First<PaginationInfoDTO>().RowCount;
                        result.PageSize = _paginationInfoDTO.First<PaginationInfoDTO>().PageSize;
                        result.CurrentPage = _paginationInfoDTO.First<PaginationInfoDTO>().PageIndex;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[GetPartnerListingSearchResultQuery] {ex.Message}");
                    throw ex;
                }
                //return Result.Failure<PagedResult<PartnerListingSearchResultOutputDTO>>(
                //            $"Get partner listing results failed."
                //        );
            }
        }
    }
}
