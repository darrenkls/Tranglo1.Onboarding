using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.KYCPartnerKYCApprovalList, UACAction.View)]
    [Permission(Permission.KYCManagementPartnerKYCApprovalList.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { })]
    internal class GetKYCApprovalRequisitionListQuery : BaseQuery<PagedResult<PartnerKYCStatusRequisitionListingOutputDTO>>
    {
        public PartnerKYCStatusRequisitionListingInputDTO InputDTO { get; set; }
        public string EntityCode { get; set; }
        public int? IsComplianceApproval { get; set; }
        public int? AdminSolution { get; set; }
        public long? CollectionTierCode { get; set; }
        public int? CustomerTypeCode { get; set; }
        public PagingOptions PagingOptions = new PagingOptions();

        public override Task<string> GetAuditLogAsync(PagedResult<PartnerKYCStatusRequisitionListingOutputDTO> result)
        {
            return Task.FromResult("Searched Partner KYC Approvals");
        }

        internal class GetKYCApprovalRequisitionListQueryHandler : IRequestHandler<GetKYCApprovalRequisitionListQuery, PagedResult<PartnerKYCStatusRequisitionListingOutputDTO>>
        {
            private readonly IConfiguration _config;

            private class PartnerKYCRequisitionList : PartnerKYCStatusRequisitionListingOutputDTO
            {
                public int FinalApprovalStatus { get; set; }
                public string Level1Remarks { get; set; }
                public string Level2Remarks { get; set; }
                public string Remarks { get; set; }
            }

            public GetKYCApprovalRequisitionListQueryHandler(IConfiguration config)
            {
                _config = config;

            }

            public async Task<PagedResult<PartnerKYCStatusRequisitionListingOutputDTO>> Handle(GetKYCApprovalRequisitionListQuery request, CancellationToken cancellationToken)
            {
                //this happen when user click on reset button
                if (request.AdminSolution is null)
                {
                    request.AdminSolution = 1;
                }
                if(request.IsComplianceApproval is null)
                {
                    request.IsComplianceApproval = 0;
                }
                
                var _connectionString = _config.GetConnectionString("DefaultConnection");
                var query = request.InputDTO;
                PagedResult<PartnerKYCStatusRequisitionListingOutputDTO> result = new PagedResult<PartnerKYCStatusRequisitionListingOutputDTO>();
                IEnumerable<PartnerKYCRequisitionList> kycRequisition;
                IEnumerable<PaginationInfoDTO> _paginationInfoDTO;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var reader = await connection.QueryMultipleAsync(
                        "dbo.GetPartnerKYCRequisitionListingPaging",
                        new
                        {
                            query.RequisitionCode,
                            query.KYCStatusCode,
                            @TrangloEntity = request.EntityCode,
                            query.BusinessProfileCode,
                            query.CreatedBy,
                            query.L1Approval,
                            query.L2Approval,
                            query.ApprovalStatusCode,
                            query.CreatedDateStart,
                            query.CreatedDateEnd,
                            query.L1ApprovalDateStart,
                            query.L1ApprovalDateEnd,
                            query.L2ApprovalDateStart,
                            query.L2ApprovalDateEnd,
                            request.AdminSolution,
                            request.IsComplianceApproval,
                            request.CollectionTierCode,
                            request.CustomerTypeCode,
                            @OrderBy = request.PagingOptions.SortExpression,
                            @PageNo = request.PagingOptions.PageIndex,
                            request.PagingOptions.PageSize,
                            @SortDirection = request.PagingOptions.Direction

                        }
                       , commandType: System.Data.CommandType.StoredProcedure) ;
                    kycRequisition = await reader.ReadAsync<PartnerKYCRequisitionList>();
                    _paginationInfoDTO = await reader.ReadAsync<PaginationInfoDTO>();

                }

                List<PartnerKYCStatusRequisitionListingOutputDTO> outputDTOs = new List<PartnerKYCStatusRequisitionListingOutputDTO>();

                foreach(var requisition in kycRequisition)
                {

                    var outputRequisition = new PartnerKYCStatusRequisitionListingOutputDTO
                    {
                        RequisitionCode = requisition.RequisitionCode,
                        RequisitionStatus = requisition.RequisitionStatus,
                        RequestType = "Update KYC Status",
                        KYCStatus = requisition.KYCStatus,
                        CompanyName = requisition.CompanyName,
                        CompanyRegistrationName = requisition.CompanyRegistrationName,
                        TrangloEntity = requisition.TrangloEntity,
                        CORemarks = requisition.Remarks,
                        ApproverRemarks = $"{requisition.Level1Remarks} {Environment.NewLine} {requisition.Level2Remarks}",
                        BusinessProfileCode = requisition.BusinessProfileCode,
                        Country = requisition.Country,
                        ApprovalLevel = requisition.ApprovalLevel,
                        CreatedBy = requisition.CreatedBy,
                        CreatedDate = requisition.CreatedDate,
                        Level1ApprovedBy = requisition.Level1ApprovedBy,
                        Level1CreatedDate = requisition.Level1CreatedDate.HasValue ? requisition.Level1CreatedDate.Value.ToUniversalTime() : requisition.Level1CreatedDate,
                        Level2ApprovedBy = requisition.Level2ApprovedBy,
                        Level2CreatedDate = requisition.Level2CreatedDate.HasValue ? requisition.Level2CreatedDate.Value.ToUniversalTime() : requisition.Level2CreatedDate,
                        SolutionCode = requisition.SolutionCode,
                        CollectionTierCode = requisition.CollectionTierCode,
                        CollectionTierDescription = requisition.CollectionTierDescription,
                        CustomerTypeCode = requisition.CustomerTypeCode,
                        CustomerTypeDescription = requisition.CustomerTypeDescription
                    };

                    ApprovalWorkflowEngine.Enum.ApprovalStatus approvalStatusEnum = (ApprovalWorkflowEngine.Enum.ApprovalStatus)Convert.ToInt32(requisition.FinalApprovalStatus);
                    outputRequisition.ApprovalStatus = approvalStatusEnum.ToString();

                    if (requisition.RequisitionStatus != (int) ApprovalWorkflowEngine.Enum.RequisitionStatus.Completed)
                    {
                        outputRequisition.ApprovalStatus = $"Pending L{requisition.ApprovalLevel} Approval";
                    }

                    outputDTOs.Add(outputRequisition);
                }
                
                
                result.Results = outputDTOs;
                result.RowCount = _paginationInfoDTO.First<PaginationInfoDTO>().RowCount;
                result.PageSize = _paginationInfoDTO.First<PaginationInfoDTO>().PageSize;
                result.CurrentPage = _paginationInfoDTO.First<PaginationInfoDTO>().PageIndex;


                return result;
            }
        
        }
    }
}
