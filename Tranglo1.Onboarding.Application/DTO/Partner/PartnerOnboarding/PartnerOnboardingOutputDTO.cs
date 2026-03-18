using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerOnboarding
{
    public class PartnerOnboardingOutputDTO
    {
       public long? ProfileOnboardWorkflowStatusCode {get; set;}
       public long? KYCOnboardWorkflowStatusCode { get; set; }
       public long? AgreementOnboardWorkflowCode { get; set; }
       public long? APIIntegrationOnboardWorkflowCode { get; set; }
       public bool CheckingOnAgreementStatusStartEndDate { get; set; }
       public long? AgreementStatus { get; set; }
       public DateTime? AgreementStartDate { get; set; }
       public DateTime? AgreementEndDate { get; set; }


    }
}
