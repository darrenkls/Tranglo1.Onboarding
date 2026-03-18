using Microsoft.Extensions.Configuration;
using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using System.Data;
using Tranglo1.Onboarding.Domain.Common;
using System.Linq.Expressions;
using Tranglo1.Onboarding.Application.DTO.Watchlist;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.Compliance, UACAction.View)]
    internal class GetKYCWatchlistDetailsHistoryQuery : PagingOptions ,IRequest<PagedResult<KYCWatchlistDetailsHistoryOutputDTO>>
    {
        public int ScreeningInputCode { get; set; }

        public class GetKYCWatchlistDetailsHistoryQueryHandler : IRequestHandler<GetKYCWatchlistDetailsHistoryQuery, PagedResult<KYCWatchlistDetailsHistoryOutputDTO>>
        {
            private readonly IConfiguration _config;

            public GetKYCWatchlistDetailsHistoryQueryHandler(IConfiguration config)
            {
                _config = config;
            }

            public async Task<PagedResult<KYCWatchlistDetailsHistoryOutputDTO>> Handle(GetKYCWatchlistDetailsHistoryQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    PagedResult<KYCWatchlistDetailsHistoryOutputDTO> result = new PagedResult<KYCWatchlistDetailsHistoryOutputDTO>();
                    var _sortExpression = string.IsNullOrEmpty(request.SortExpression) ? "" : request.SortExpression + " " + (request.Direction == SortDirection.Ascending ? "" : "DESC");
                    var _connectionString = _config.GetConnectionString("DefaultConnection");

                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        var reader = await connection.QueryMultipleAsync(
                           "GetWatchlistHistory",
                           new
                           {
                               ScreeningInputCode = request.ScreeningInputCode,
                               PageIndex = request.PageIndex,
                               PageSize = request.PageSize,
                               sortExpression = _sortExpression
                           },
                           null, null, CommandType.StoredProcedure);
                        result.Results = await reader.ReadAsync<KYCWatchlistDetailsHistoryOutputDTO>();
                        var documentsDownloadDTO = await reader.ReadAsync<WatchlistDocumentsDownloadDTO>();

                        IEnumerable<PaginationInfoDTO> _paginationInfoDTO = await reader.ReadAsync<PaginationInfoDTO>();
                        result.RowCount = _paginationInfoDTO.First<PaginationInfoDTO>().RowCount;
                        result.PageSize = _paginationInfoDTO.First<PaginationInfoDTO>().PageSize;
                        result.CurrentPage = _paginationInfoDTO.First<PaginationInfoDTO>().PageIndex;

                        foreach (var documentDTO in result.Results)
                        {
                            var documents = documentsDownloadDTO
                                                .Where(x => x.WatchlistCode == documentDTO.WatchlistCode)
                                                .ToList();

                            documentDTO.DocumentId = documents.Select(x => x.DocumentId).ToArray();

                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
