using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerAgreementStatusOutputDTO
    {
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public int? PartnerAgreementStatusCode { get; set; }
        public string PartnerAgreementStatus { get; set; }
    }
}
