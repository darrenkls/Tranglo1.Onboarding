using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate
{
    public class KYCSubModuleReview : Entity
    {
        public int BusinessProfileCode { get; set; }
        public int KYCCategoryCode { get; set; }
        public ReviewResult ReviewResult { get; set; }
        public BusinessProfile BusinessProfile { get; set; }
        public KYCCategory KYCCategory { get; set; }
        public string Remarks { get; set; }
    }
}
