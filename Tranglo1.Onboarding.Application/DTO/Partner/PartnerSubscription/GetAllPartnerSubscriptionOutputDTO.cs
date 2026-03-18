using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription
{
    public class GetAllPartnerSubscriptionOutputDTO
    {
        public List<PartnerSubscriptionList> partnerSubscriptionLists { get; set; }
        
    }
    public class PartnerSubscriptionList
    {
       public long SolutionCode { get; set; }
       public string SolutionDescription { get; set; } 
       
    }
}

