using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO
{
    public class GetKYCConnectUserSummaryOutputDTO
    {
        public long CategoryCode { get; set; }
        public string Category { get; set; }
        public string CategoryDisplay { get; set; }
        public long Status { get; set; }
        public string StatusDesc { get; set; }
        public List<KYCConnectSubCategory> SubCategories { get; set; }
    }

    public class KYCConnectSubCategory
    {
        public long SubCategoryCode { get; set; }
        public string SubCategoryDesc { get; set; }
        public long Status { get; set; }
        public string StatusDesc { get; set; }
    }
}
