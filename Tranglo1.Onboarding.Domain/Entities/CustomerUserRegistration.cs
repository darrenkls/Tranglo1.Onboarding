namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerUserRegistration
    {
        public long CustomerUserRegistrationId { get; set; }
        public string LoginId { get; set; }
        public string CompanyName { get; set; }
        public string SignUpCode { get; set; }
        public int? SolutionCode { get; set; }
        public int? CustomerTypeCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public PartnerRegistrationLeadsOrigin PartnerRegistrationLeadsOrigin { get; set; }
        public string OtherPartnerRegistrationLeadsOrigin { get; set; }

        public CustomerUserRegistration(Email loginId, CompanyName companyName, int? solutionCode = null, int? customerTypeCode = null, PartnerRegistrationLeadsOrigin leadsOrigin = null, string otherLeadsOrigin = null)
        {
            LoginId = loginId.Value;
            CompanyName = companyName.Value;
            SolutionCode = solutionCode;
            CustomerTypeCode = customerTypeCode;
            PartnerRegistrationLeadsOrigin = leadsOrigin;
            OtherPartnerRegistrationLeadsOrigin = otherLeadsOrigin;
        }

        public CustomerUserRegistration(Email loginId, CompanyName companyName, int businessProfile, int solutionCode)
        {
            LoginId = loginId.Value;
            CompanyName = companyName.Value;
            BusinessProfileCode = businessProfile;
            SolutionCode = solutionCode;
        }

        public CustomerUserRegistration(Email loginId, string signUpCode, string companyName)
        {
            LoginId = loginId.Value;
            SignUpCode = signUpCode;
            CompanyName = companyName;
        }

        public CustomerUserRegistration(Email loginId, string signUpCode, string companyName, int businessProfile, PartnerRegistrationLeadsOrigin leadsOrigin = null, string otherLeadsOrigin = null)
        {
            LoginId = loginId.Value;
            SignUpCode = signUpCode;
            CompanyName = companyName;
            BusinessProfileCode = businessProfile;
            PartnerRegistrationLeadsOrigin = leadsOrigin;
            OtherPartnerRegistrationLeadsOrigin = otherLeadsOrigin;
        }

        private CustomerUserRegistration() { }
    }
}
