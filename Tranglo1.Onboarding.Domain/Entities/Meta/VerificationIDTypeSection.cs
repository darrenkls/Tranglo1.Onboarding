using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities.Meta
{
    public class VerificationIDTypeSection : Entity
    {
        public string Description { get; set; }
        public VerificationIDType VerificationIDType { get; set; }
    }
}
