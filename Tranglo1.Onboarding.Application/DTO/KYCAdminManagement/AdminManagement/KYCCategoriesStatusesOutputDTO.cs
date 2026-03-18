using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.ComplianceOfficers
{
    public class KYCCategoriesStatusesOutputDTO
    {
        public long KYCCategoryCode { get; set; }
        public string KYCCategoryDescription { get; set; }
        public DateTime? UserUpdatedDate { get; set; }
        public DateTime? LastReviewedDate { get; set; }
        public long ReviewResultCode { get; set; }
        public string ReviewResultDescription { get; set; }
    }
}
