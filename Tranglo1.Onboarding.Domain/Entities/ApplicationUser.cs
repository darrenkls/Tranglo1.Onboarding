using CSharpFunctionalExtensions;
using System;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    /// <summary>
    /// Stub — proper definition belongs in Tranglo1.Identity.Contracts NuGet package.
    /// Replace with Contracts type once the package is updated.
    /// </summary>
    public class ApplicationUser : Entity
    {
        public string UserName { get; set; }
        public Email Email { get; set; }
        public FullName FullName { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public AccountStatus AccountStatus { get; set; }
    }
}
