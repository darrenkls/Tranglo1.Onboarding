using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class UpdatePartnerTermsAndConditionsAcceptanceDateCommandOutputDTO
    {
        public long PartnerCode { get; set; }
        public DateTime? TermsAcceptanceDate { get; set; }
    }
}
