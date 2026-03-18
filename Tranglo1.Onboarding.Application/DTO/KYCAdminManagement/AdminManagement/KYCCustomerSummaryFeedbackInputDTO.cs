using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCCustomerSummaryFeedbackInputDTO
    {
        public int KYCCustomerSummaryFeedbackCode { get; set; }
        public long KYCCategoryCode { get; set; }
        [MaxLength(500, ErrorMessage = "Feedback To Tranglo maximum length is 500 characters")]
        public string FeedbackToTranglo { get; set; }
    }
}
