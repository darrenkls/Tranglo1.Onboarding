using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerListingsInputDTO
    {
        public int PartnerCode { get; set; }
        public int PartnerTypeCode { get; set; }
        public int SolutionCode { get; set; }
        public int CurrentAPIEnvironmentCode { get; set; }
        public int AgreementStatusCode { get; set; }
        public int PartnerStatusCode { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
