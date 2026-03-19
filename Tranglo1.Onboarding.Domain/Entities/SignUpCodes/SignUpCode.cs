using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.SignUpCodes
{
    /// <summary>
    /// Stub — proper definition may belong in Tranglo1.Identity.Contracts NuGet package.
    /// Replace with Contracts type once the package is updated.
    /// </summary>
    public class SignUpCode : Entity
    {
        public string Code { get; set; }
        public long PartnerCode { get; set; }
        public string CompanyName { get; set; }
    }
}
