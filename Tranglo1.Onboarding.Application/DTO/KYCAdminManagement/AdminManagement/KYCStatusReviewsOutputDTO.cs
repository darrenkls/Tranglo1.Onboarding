using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCStatusReviewsOutputDTO
    {
        public bool IsKYCStatusUpdated { get; set; }
        public bool IsRequisitionCreated { get; set; }
        public string RequisitionCode { get; set; }
        public List<KYCSubModuleOutputDTO> KYCSubModuleReview { get; set; }
        public Guid? ReviewAndFeedbackConcurrencyToken { get; set; }
        public DateTime? ReviewAndFeedbackConcurrentLastModified { get; set; }
        public class KYCSubModuleOutputDTO
        {
            public long KYCCategoryCode { set; get; }
            public string KYCCategoryDescription { set; get; }
            public DateTime? UserUpdatedDate { set; get; }
            public DateTime? LastReviewedDate { set; get; }
            public long ReviewResultCode { set; get; }
            public string ReviewResultDescription { set; get; }
        }


    }
}
