using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    /// <summary>
    /// Stub — proper definition belongs in Tranglo1.Identity.Contracts NuGet package.
    /// Replace with Contracts type once the package is updated.
    /// </summary>
    public class CustomerUser : ApplicationUser
    {
        public CountryMeta CountryMeta { get; set; }
        public CompanyUserBlockStatus CompanyUserBlockStatus { get; set; }
    }
}
