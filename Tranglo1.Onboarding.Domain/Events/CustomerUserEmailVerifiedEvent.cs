using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Events
{
    public class CustomerUserEmailVerifiedEvent : DomainEvent
    {
        public string CustomerEmail { get; private set; }
        public int? SolutionCode { get; set; }
        public bool IsMultipleSolution { get; set; }

        public CustomerUserEmailVerifiedEvent(string email, int? solutionCode, bool isMultipleSolution)
        {
            CustomerEmail = email;
            SolutionCode = solutionCode;
            IsMultipleSolution = isMultipleSolution;
        }
    }
}
