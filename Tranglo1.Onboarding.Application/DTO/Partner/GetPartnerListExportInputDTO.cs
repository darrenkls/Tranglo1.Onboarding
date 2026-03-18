using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class GetPartnerListExportInputDTO
    {
        public GetPartnerListExportSearchInputDTO Search { get; set; }
        public GetPartnerListExportColumnInputDTO Column { get; set; }
    }

    public class GetPartnerListExportSearchInputDTO
    {
        public string PartnerName { get; set; }
        public string TradeName { get; set; }
        public string PartnerType { get; set; }
        public string CountryISO2 { get; set; }
        public string Agent { get; set; }
        public long? AgreementStatusCode { get; set; }
        public string AgreementStartDate { get; set; }
        public string AgreementEndDate { get; set; }
        public long? WorkFlowStatusCode { get; set; }
        public long? StatusCode { get; set; }
        public long? EnvironmentCode { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public string SortExpression { get; set; }
        public SortDirection SortDirection { get; set; }
        public long? AdminSolution { get; set; }
        public int? KycApprovalStatusCode { get; set; }
        public int? KycStatusCode { get; set; }
        public int? KycReminderStatusCode { get; set; }
        public int? LeadsOriginCode { get; set; }
    }

    public class GetPartnerListExportColumnInputDTO
    {
        public bool? PartnerName {  get; set; }
        public bool? TradeName { get; set; }
        public bool? Country { get; set; }
        public bool? RegistrationDate { get; set; }
        public bool? Agent {  get; set; }
        public bool? AgreementStatus {  get; set; }
        public bool? AgreementStartDate { get; set; }
        public bool? AgreementEndDate { get; set; }
        public bool? Solution { get; set; }
        public bool? Entity {  get; set; }
        public bool? PartnerType { get; set; }
        public bool? Environment {  get; set; }
        public bool? WorkflowStatus { get; set; }
        public bool? KycStatus { get; set; }
        public bool? ReminderStatus {  get; set; }
        public bool? PartnerKycApprovalStatus { get; set; }
        public bool? Status { get; set; }
        public bool? LeadsOrigin { get; set; }
    }
}
