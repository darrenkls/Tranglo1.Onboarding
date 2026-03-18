using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner
{
    public class PartnerListingSearchResultOutputDTO
    {
        public long PartnerCode { get; set; }
        public long? PartnerSubscriptionCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerTypeDescription { get; set; }
        public string SolutionDescription { get; set; }
        public string EntityTypeDescription { get; set; }
        public string CurrentAPIEnvironmentDescription { get; set; }
        public string AgreementStatusDescription { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string PartnerStatusDescription { get; set; }
    }
}
