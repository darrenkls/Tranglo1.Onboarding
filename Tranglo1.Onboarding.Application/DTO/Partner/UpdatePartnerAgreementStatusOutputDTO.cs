using System;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class UpdatePartnerAgreementStatusOutputDTO
    {
        public long PartnerCode { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public int AgreementStatus { get; set; }
    }
}
