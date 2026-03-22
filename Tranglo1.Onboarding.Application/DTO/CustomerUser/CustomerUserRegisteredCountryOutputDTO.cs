namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class CustomerUserRegisteredCountryOutputDTO
    {
        public int CustomerUserBusinessProfileCode { get; set; }
        public int CustomerUserId { get; set; }
        public int CountryCode { get; set; }
        public string CountryISO2 { get; set; }
        public string CountryDescription { get; set; }
        public int CompanyRegisteredCountryCode { get; set; }
        public string CompanyRegisteredCountryISO2 { get; set; }
        public string CompanyRegisteredCountryDescription { get; set; }
        public bool IsTINMandatory { get; set; }
    }
}
