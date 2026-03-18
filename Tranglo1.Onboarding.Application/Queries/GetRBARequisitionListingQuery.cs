using Dapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine.Enum;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.DTO.RBA;


namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetRBARequisitionListingQuery : PagingOptions, IRequest<PagedResult<GetRBARequisitionListingOutputDTO>>
    { 
        public GetRBARequisitionListingInputDTO InputDTO;
        public string TrangloEntityCode { get; set; }

        public class GetRBARequisitionListingQueryHandler : IRequestHandler<GetRBARequisitionListingQuery, PagedResult<GetRBARequisitionListingOutputDTO>>
        {
      
            private readonly IConfiguration _config;

            public GetRBARequisitionListingQueryHandler(IConfiguration config)
            {
                _config = config;
            
            }

            public async Task<PagedResult<GetRBARequisitionListingOutputDTO>> Handle(GetRBARequisitionListingQuery request, CancellationToken cancellationToken)
            {
                var result = new PagedResult<GetRBARequisitionListingOutputDTO>();
                IEnumerable<PaginationInfoDTO> _paginationInfoDTO;

                IEnumerable<GetRBARequisitionListingOutputDTO> resultList;

                List<GetRBARequisitionListingOutputDTO> finalResultList = new List<GetRBARequisitionListingOutputDTO>();

                var storedProcedure = "RBAApprovalListingPaging";

                try
                {
                    var _connectionString = _config.GetConnectionString("DefaultConnection");
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        var reader = await connection.QueryMultipleAsync(
                            storedProcedure,
                            new
                            {
                                
                                request.InputDTO.RequisitionCode,
                                TrangloEntity = request.TrangloEntityCode,
                                request.InputDTO.SolutionCode,
                                request.InputDTO.ComplianceSettingTypeCode,
                                request.InputDTO.ComplianceRequisitionTypeCode,
                                request.InputDTO.RequestedBy,
                                request.InputDTO.ApprovedBy,
                                request.InputDTO.EditedBy,
                                ApprovalStatusCode = request.InputDTO.FinalApprovalStatusCode,
                                request.InputDTO.RequestedDurationFrom,
                                request.InputDTO.RequestedDurationTo,
                                request.InputDTO.ApprovedDurationFrom,
                                request.InputDTO.ApprovedDurationTo,
                                request.InputDTO.EditedDurationFrom,
                                request.InputDTO.EditedDurationTo,
                                @PageNo = request.PageIndex,
                                @PageSize = request.PageSize,
                                @SortDirection = request.Direction,
                                @OrderBy = request.SortExpression

                            },
                            commandType: CommandType.StoredProcedure);

                        resultList = await reader.ReadAsync<GetRBARequisitionListingOutputDTO>();
                        _paginationInfoDTO = await reader.ReadAsync<PaginationInfoDTO>();

                    }

                    foreach (var item in resultList)
                    {      
                        string approvalStatus = GetEnumNameById<ApprovalStatus>((int)item.FinalApprovalStatus);
                        string reqStatusDescription = GetEnumNameById<RequisitionStatus>((int)item.RequisitionStatus);
                        item.RequisitionStatusDescription = reqStatusDescription;
                        item.FinalApprovalStatusDescription = approvalStatus;
                        finalResultList.Add(item);
                    }

                    result.Results = finalResultList;
                    result.RowCount = _paginationInfoDTO.First<PaginationInfoDTO>().RowCount;
                    result.PageSize = _paginationInfoDTO.First<PaginationInfoDTO>().PageSize;
                    result.CurrentPage = _paginationInfoDTO.First<PaginationInfoDTO>().PageIndex;
                }
                catch (Exception ex)
                {
                    //Display error message
                    Console.WriteLine("Exception: " + ex.Message);
                }
                return result;
            }
        }
        private static string GetEnumNameById<TEnum>(int id) where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), id);
        }
    }
}