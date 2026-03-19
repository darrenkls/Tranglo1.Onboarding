using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.SignUpCodes
{
    public class SignUpCode : Entity
    {
        public string Code { get; set; }
        public long PartnerCode { get; set; }
        public string CompanyName { get; set; }
    }
}
