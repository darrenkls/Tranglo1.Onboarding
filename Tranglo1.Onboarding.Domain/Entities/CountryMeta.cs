using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CountryMeta : Enumeration
    {
        public string CountryISO2 { get; set; }
        public string CountryISO3 { get; set; }
        public string DialCode { get; set; }

        public CountryMeta() : base() { }

        public CountryMeta(int id, string name, string countryISO2, string countryISO3, string dialCode)
            : base(id, name)
        {
            CountryISO2 = countryISO2;
            CountryISO3 = countryISO3;
            DialCode = dialCode;
        }
    }
}
