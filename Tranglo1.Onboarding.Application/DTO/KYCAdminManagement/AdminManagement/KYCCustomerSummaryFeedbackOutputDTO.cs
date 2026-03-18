using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCCustomerSummaryFeedbackOutputDTO
    {
        public int KYCCustomerSummaryFeedbackCode { get; set; }
        public long KYCCategoryCode { get; set; }
        public string FeedbackToTranglo { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string CreatedByName { get; set; }
        public long CreatedBy { get; set; }
    }
}
