namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class GetAllBusinessProfilesOutputDTO
    {
        public int BusinessProfileCode { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistrationName { get; set; }
        public string CountryISO2 { get; set; }
        public string KYCStatusDescription { get; set; }
        public string SolutionDescription { get; set; }
    }
}
