using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerUserBusinessProfile : Entity
    {
        public CustomerUser CustomerUser { get; private set; }
        public int UserId { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }
        public int BusinessProfileCode { get; set; }
        public Environment Environment { get; set; }
        public CompanyUserAccountStatus CompanyUserAccountStatus { get; set; }
        public CompanyUserBlockStatus CompanyUserBlockStatus { get; set; }

        private CustomerUserBusinessProfile() { }

        public CustomerUserBusinessProfile(CustomerUser customerUser, BusinessProfile businessProfile)
        {
            CustomerUser = customerUser;
            BusinessProfile = businessProfile;
            UserId = customerUser.Id;
            BusinessProfileCode = businessProfile.Id;
            Environment = Environment.Staging;
            CompanyUserAccountStatus = CompanyUserAccountStatus.Active;
            CompanyUserBlockStatus = CompanyUserBlockStatus.Unblock;
        }

        public void SetCompanyUserBlockStatus(CompanyUserBlockStatus companyUserBlockStatus)
        {
            CompanyUserBlockStatus = companyUserBlockStatus;
        }

        public void SetCompanyUserAccountStatus(CompanyUserAccountStatus companyUserAccountStatus)
        {
            if (CompanyUserAccountStatus != companyUserAccountStatus)
            {
                CompanyUserAccountStatus = companyUserAccountStatus;
            }
        }
    }
}
