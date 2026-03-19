using System;
using CSharpFunctionalExtensions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class UserVerificationToken : Entity
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
    }
}
