using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCStatusReviewsInputDTO
    {

        public long KYCStatusCode { set; get; }
        public string Remarks { get; set; }
        public IEnumerable<KYCSubModuleReviewInputDTO> kycSubModuleReview { set; get;}
    }
}
