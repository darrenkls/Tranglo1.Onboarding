using CSharpFunctionalExtensions;
using Tranglo1.Onboarding.Domain.Entities.SignUpCodes;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerUserRegistration : Entity
    {
        public string SignUpCode { get; set; }
        public string CompanyName { get; set; }
        public int? SolutionCode { get; set; }
        public long? CustomerTypeCode { get; set; }
        public LeadsOrigin PartnerRegistrationLeadsOrigin { get; set; }
        public string OtherPartnerRegistrationLeadsOrigin { get; set; }
    }
}
