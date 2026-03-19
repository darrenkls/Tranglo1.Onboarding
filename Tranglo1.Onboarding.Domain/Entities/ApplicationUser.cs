using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ApplicationUser : Entity
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
