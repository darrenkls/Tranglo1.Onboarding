using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerUserBusinessProfile : Entity
    {
        public CustomerUser CustomerUser { get; set; }
        public BusinessProfileAggregate.BusinessProfile BusinessProfile { get; set; }
        public Environment Environment { get; set; }
        public CompanyUserAccountStatus CompanyUserAccountStatus { get; set; }
        public CompanyUserBlockStatus CompanyUserBlockStatus { get; set; }
    }
}
