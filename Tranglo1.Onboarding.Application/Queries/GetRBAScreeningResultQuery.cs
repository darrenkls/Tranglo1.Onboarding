using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.ApprovalWorkflowEngine;
using Tranglo1.ApprovalWorkflowEngine.Enum;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate.Requisitions;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.RBA;
using Tranglo1.Onboarding.Application.MediatR;

namespace Tranglo1.Onboarding.Application.Queries
{
    internal class GetRBAScreeningResultQuery : BaseQuery<List<RBAResultsOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public override Task<string> GetAuditLogAsync(List<RBAResultsOutputDTO> result)
        {
            return Task.FromResult<string>("View Compliance Internal Risk Rating");
        }

        public class GetRBAScreeningResultQueryHandler : IRequestHandler<GetRBAScreeningResultQuery, List<RBAResultsOutputDTO>>
        {
            private readonly IRBARepository _rbaRepository;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IPartnerRepository _partnerRepository;
            private readonly ApprovalManager<RBARequisition> _rbaApprovalManager;

            public GetRBAScreeningResultQueryHandler(IRBARepository rbaRepository, IBusinessProfileRepository businessProfileRepository, IPartnerRepository partnerRepository, ApprovalManager<RBARequisition> rbaApprovalManager)
            {
                _rbaRepository = rbaRepository;
                _businessProfileRepository = businessProfileRepository;
                _partnerRepository = partnerRepository;
                _rbaApprovalManager = rbaApprovalManager;
            }

            public async Task<List<RBAResultsOutputDTO>> Handle(GetRBAScreeningResultQuery request, CancellationToken cancellationToken)
            {
                List<RBAResultsOutputDTO> output = new List<RBAResultsOutputDTO>();

                try
                {
                    var rbaLastestScreeningInput = await _rbaRepository.GetLatestRBAScreeningInputByBusinessProfileCodeAsync(request.BusinessProfileCode);

                    var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);

                    var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);

                    string customerName = businessProfile.CompanyName + ", " + partner.Id;
                    string pendingStatus = GetEnumNameById<ApprovalWorkflowEngine.Enum.ApprovalStatus>(((int)ApprovalWorkflowEngine.Enum.ApprovalStatus.Pending));
                    int pendingStatusCode = (int)ApprovalWorkflowEngine.Enum.ApprovalStatus.Pending;

                    foreach (var item in rbaLastestScreeningInput.RBAList)
                    {
                        var riskRankingCode = Enumeration.FindByName<RiskRanking>(item.RiskRanking);
                        RBAResultsOutputDTO outputDto = new RBAResultsOutputDTO();

                        outputDto.CustomerName = customerName;
                        outputDto.RBACode = item.Id;
                        outputDto.DisplayDateReportRuns = item.RBAScreeningDate;
                        outputDto.RBAScreeningDate = item.RBAScreeningDate;
                        outputDto.RiskScore = item.RiskScore;
                        outputDto.RiskRanking = item.RiskRanking;
                        outputDto.RiskRankingCode = riskRankingCode.Id;
                        outputDto.InternalRiskRanking = "NA";
                        outputDto.TrangloEntity = item.TrangloEntity;

                        var existingRequisition = await _rbaRepository.GetLatestRBARequisitionsByRBACodeAsync(item.Id);
                        if (existingRequisition != null)
                        {
                            if (existingRequisition.RequisitionStatus == ApprovalWorkflowEngine.Enum.RequisitionStatus.New)
                            {
                                outputDto.FinalApprovalStatusDescription = pendingStatus;
                                outputDto.FinalApprovalStatus = pendingStatusCode;
                            }
                            else
                            {
                                var requisitionApprovalStatus = await _rbaApprovalManager.GetRequisitionHistoryByCodeAsync(existingRequisition.RequisitionCode);
                                var latestStatus = requisitionApprovalStatus
                                    .Where(status => status.ApprovalStatus == ApprovalWorkflowEngine.Enum.ApprovalStatus.Approved
                                                  || status.ApprovalStatus == ApprovalWorkflowEngine.Enum.ApprovalStatus.Rejected)
                                    .OrderByDescending(status => status.CreatedDate)
                                    .FirstOrDefault();

                                if (latestStatus != null)
                                {
                                    string approvalStatus = GetEnumNameById<ApprovalWorkflowEngine.Enum.ApprovalStatus>((int)latestStatus.ApprovalStatus);
                                    int approvalStatusCode = (int)Enum.Parse(typeof(ApprovalWorkflowEngine.Enum.ApprovalStatus), latestStatus.ApprovalStatus.ToString());

                                    outputDto.FinalApprovalStatusDescription = approvalStatus;
                                    outputDto.FinalApprovalStatus = approvalStatusCode;

                                    if (latestStatus.ApprovalStatus == ApprovalWorkflowEngine.Enum.ApprovalStatus.Approved)
                                    {
                                        outputDto.InternalRiskRanking = item.RiskRanking;
                                    }
                                }
                            }
                        }

                        outputDto.rbaDetails = new List<ResultDesc>();


                        foreach (var evaluation in item.EvaluationRules)
                        {
                            ResultDesc paramDesc = new ResultDesc();

                            if (evaluation.Parameter != null)
                            {
                                paramDesc.TemplateDescription = evaluation.Template.Replace(evaluation.Parameter.Name, evaluation.Parameter.Description);
                                paramDesc.ActualValue = evaluation.ActualValue;
                                paramDesc.Score = evaluation.Score;
                                paramDesc.CriticalRanking = evaluation.CriticalRanking;
                            }

                            outputDto.rbaDetails.Add(paramDesc);
                        }

                        output.Add(outputDto);
                    }
                }
                catch (Exception ex)
                {

                }

                return output;
            }
        }
        private static string GetEnumNameById<TEnum>(int id) where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), id);
        }
    }
}
