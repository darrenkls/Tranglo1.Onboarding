using CSharpFunctionalExtensions;
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
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Queries
{
    //[Permission(PermissionGroupCode.PartnerManagement, UACAction.View)]
    [Permission(Permission.ManagePartner.Action_View_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { })]
    internal class GetPartnerListingQuery : BaseQuery<PagedResult<PartnerListingOutputDTO>>
    {
        public string PartnerName { get; set; }
        public string TradeName { get; set; }
        public string Entity { get; set; }
        public string PartnerType { get; set; }
        public string CountryISO2 { get; set; }
        public string Agent { get; set; }
        public long AgreementStatusCode { get; set; }
        public string AgreementStartDate { get; set; }
        public string AgreementEndDate { get; set; }
        public long WorkFlowStatusCode { get; set; }
        public long StatusCode { get; set; }
        public long EnvironmentCode { get; set; }
        public string UserBearerToken { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public int? KYCApprovalStatusCode { get; set; }
        public int? KYCStatusCode { get; set; }
        public int? KYCReminderStatusCode { get; set; }
        public int? LeadsOriginCode { get; set; }
        public PagingOptions PagingOptions = new PagingOptions();
        public override Task<System.String> GetAuditLogAsync(PagedResult<PartnerListingOutputDTO> result)
        {
            return Task.FromResult("Searched partner records");
        }

        public class GetPartnerListingQueryHandler : IRequestHandler<GetPartnerListingQuery, PagedResult<PartnerListingOutputDTO>>
        {
            private readonly IConfiguration _config;
            private readonly PartnerService _partnerService;
            private readonly IPartnerRepository _partnerRepository;

            public GetPartnerListingQueryHandler(IConfiguration config, PartnerService partnerService, IPartnerRepository partnerRepository)
            {
                _config = config;
                _partnerService = partnerService;
                _partnerRepository = partnerRepository;
            }

            public async Task<PagedResult<PartnerListingOutputDTO>> Handle(GetPartnerListingQuery request, CancellationToken cancellationToken)
            {
                PagedResult<PartnerListingOutputDTO> result = new PagedResult<PartnerListingOutputDTO>();
                IEnumerable<PartnerListingOutputDTO> partnerlisting;
                IEnumerable<Subscription> subscriptions;

                var _connectionString = _config.GetConnectionString("DefaultConnection");
                //IEnumerable<PartnerListingOutputDTO> partnerListing_;
                long? solutionCodeInput = null;

                if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
                {
                    solutionCodeInput = Solution.Business.Id;

                }
                else if (request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id)
                {
                    solutionCodeInput = Solution.Connect.Id;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var _sortExpression = string.IsNullOrEmpty(request.PagingOptions.SortExpression) ? "" : request.PagingOptions.SortExpression + " " + (request.PagingOptions.Direction == SortDirection.Ascending ? "" : "DESC");
                    var reader = await connection.QueryMultipleAsync(
                        "dbo.GetPartnerListing",
                        new
                        {
                            SolutionCode = solutionCodeInput,
                            PageIndex = request.PagingOptions.PageIndex,
                            PageSize = request.PagingOptions.PageSize,
                            PartnerName = request.PartnerName,
                            TradeName = request.TradeName,
                            Entity = request.Entity,
                            PartnerType = request.PartnerType,
                            CountryISO2 = request.CountryISO2,
                            Agent = request.Agent,
                            AgreementStatusCode = request.AgreementStatusCode,
                            EnvironmentCode = request.EnvironmentCode,
                            startAgreementDate = request.AgreementStartDate,
                            endAgreementDate = request.AgreementEndDate,
                            WorkFlowStatusCode = request.WorkFlowStatusCode,
                            StatusCode = request.StatusCode,
                            KYCApprovalStatusCode = request.KYCApprovalStatusCode,
                            KYCStatusCode = request.KYCStatusCode,
                            KYCReminderStatusCode = request.KYCReminderStatusCode,
                            sortExpression = _sortExpression,
                            LeadsOriginCode = request.LeadsOriginCode,
                        },
                        null, null, CommandType.StoredProcedure);

                    //result.Results = await reader.ReadAsync<PartnerListingOutputDTO>();
                    partnerlisting = await reader.ReadAsync<PartnerListingOutputDTO>();
                    subscriptions = await reader.ReadAsync<Subscription>();
                    IEnumerable<PaginationInfoDTO> _paginationInfoDTO = await reader.ReadAsync<PaginationInfoDTO>();
                    result.RowCount = _paginationInfoDTO.First<PaginationInfoDTO>().RowCount;
                    result.PageSize = _paginationInfoDTO.First<PaginationInfoDTO>().PageSize;
                    result.CurrentPage = _paginationInfoDTO.First<PaginationInfoDTO>().PageIndex;
                }

                foreach (var p in partnerlisting)
                {
                    ApprovalWorkflowEngine.Enum.ApprovalStatus approvalStatusEnum = (ApprovalWorkflowEngine.Enum.ApprovalStatus)Convert.ToInt32(p.FinalApprovalStatus);
                    p.ApprovalStatus = approvalStatusEnum.ToString();

                    if (p.ApprovalLevel == 0)
                    {
                        p.ApprovalLevel = 1;
                    }
                    if (p.RequisitionStatus != (int)ApprovalWorkflowEngine.Enum.RequisitionStatus.Completed)
                    {
                        p.ApprovalStatus = $"Pending L{p.ApprovalLevel} Approval";
                    }

                    p.Subscriptions = subscriptions.Where(x => x.PartnerCode == p.PartnerCode).OrderBy(x => x.PartnerSubscriptionCode).ToList();
                    p.FullLeadsOriginDescription = String.Join(" - ", new string[] { p.LeadsOriginDescription, p.OtherLeadsOrigin }
                        .Where(x => !String.IsNullOrEmpty(x)));
                    //if (p.AgreementEndDate != null && p.AgreementEndDate < DateTime.UtcNow)
                    //{
                    //    p.AgreementStatus = "Expired";
                    //}

                    //#39856, YY say can let it call partnerService first
                    //max 20 loop only
                    //note written by JY
                    foreach (var s in p.Subscriptions)
                    {
                        var partnerProfile = await _partnerService.GetPartnerRegistrationByCodeAsync(s.PartnerCode);
                        var subscription = await _partnerRepository.GetSubscriptionAsync(s.PartnerSubscriptionCode);
                        WorkflowStatus partnerKYCProfile = new WorkflowStatus();
                        Result<OnboardWorkflowStatus> profileStatus = new OnboardWorkflowStatus();
                        var AgreementWorkFlow = Enumeration.FindById<OnboardWorkflowStatus>(partnerProfile.AgreementOnboardWorkflowStatusCode.GetValueOrDefault());
                        var APIIntegrationWorkFlow = Enumeration.FindById<OnboardWorkflowStatus>(subscription.APIIntegrationOnboardWorkflowStatusCode.GetValueOrDefault());

                        if (request.AdminSolution == Solution.Connect.Id)
                        {
                            partnerKYCProfile = await _partnerService.GetPartnerKYCStatus(s.PartnerCode);
                            profileStatus = await _partnerService.GetPartnerProfileStatus(s.PartnerCode, s.PartnerSubscriptionCode);
                        }
                        else if (request.AdminSolution == Solution.Business.Id)
                        {
                            partnerKYCProfile = await _partnerService.GetAdminBusinessKYCStatus(s.PartnerCode);
                            profileStatus = await _partnerService.GetBusinessPartnerProfileStatus(s.PartnerCode, s.PartnerSubscriptionCode);
                        }

                        var AgreementStatusText = partnerProfile.AgreementOnboardWorkflowStatusCode == null ? "Pending" : AgreementWorkFlow.Name;
                        var ApiStatusText = subscription.APIIntegrationOnboardWorkflowStatusCode == null ? "Pending" : APIIntegrationWorkFlow.Name; //per subscription
                        var KYCStatusText = partnerKYCProfile == null ? "Pending" : partnerKYCProfile.Name;
                        var profileStatusText = profileStatus.IsFailure ? "Pending" : profileStatus.Value.Name; //per subscription
                        s.WorkFlowStatus = $"Profile - {profileStatusText}, <br>" +
                            $"KYC - {KYCStatusText}, <br>" +
                            $"API - {ApiStatusText}, <br>" +
                            $"Agreement - {AgreementStatusText}, <br>" +
                            $"Onboard - {profileStatus.Value.Name} <br>";
                    }
                }

                // TBT 1791
                // Reorder partnerlisting: partners with at least one active subscription first, then by RegistrationDate descending
                partnerlisting = partnerlisting
                    .OrderByDescending(p => p.Subscriptions != null && p.Subscriptions.Any(s => s.StatusCode == PartnerAccountStatusType.Active.Id))
                    .ThenByDescending(p => p.Subscriptions != null && p.Subscriptions.Any(s => s.StatusCode == PartnerAccountStatusType.Inactive.Id))
                    .ThenByDescending(p => p.Subscriptions != null && p.Subscriptions.Any(s => s.StatusCode == PartnerAccountStatusType.Rejected.Id))
                    .ThenByDescending(p => p.RegistrationDate)
                    .ToList();

                result.Results = partnerlisting;
                return result;
            }
        }
    }
}