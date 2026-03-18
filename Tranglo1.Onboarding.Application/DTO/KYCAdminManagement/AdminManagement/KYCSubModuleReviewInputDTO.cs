using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCSubModuleReviewInputDTO
    {
        public long KYCCategoryCode { set; get; }
        public long ReviewResultCode { set; get; }
    }
}
