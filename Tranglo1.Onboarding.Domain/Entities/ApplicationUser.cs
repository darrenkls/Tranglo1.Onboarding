using System;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    /// <summary>
    /// Base user class for both internal (TrangloStaff) and external (CustomerUser) users.
    /// <c>Id</c> is the unique identifier (integer PK).
    /// <c>LoginId</c> is the login identifier. For internal users this is the AD account (e.g. xxx@tranglo.net).
    /// </summary>
    public abstract class ApplicationUser : AggregateRoot
    {
        protected ApplicationUser() { }

        public ApplicationUser(FullName fullName, Email email)
        {
            SetName(fullName);
            SetEmail(email);
        }

        public ApplicationUser(long accountStatus) { }

        public FullName FullName { get; private set; }
        public AccountStatus AccountStatus { get; private set; }
        public long AccountStatusCode { get; protected set; }
        public string Timezone { get; set; }
        public virtual DateTimeOffset? LockoutEnd { get; private set; }
        public virtual bool TwoFactorEnabled { get; private set; }
        public virtual ContactNumber ContactNumber { get; private set; }
        public virtual string ConcurrencyStamp { get; private set; }
        public virtual string SecurityStamp { get; private set; }
        public virtual Email Email { get; protected set; }
        public virtual string LoginId { get; protected set; }
        public virtual bool LockoutEnabled { get; private set; }
        public virtual int AccessFailedCount { get; private set; }
        public virtual bool? IsResetMFA { get; private set; }

        public void SetName(FullName fullName)
        {
            FullName = fullName;
        }

        public virtual void SetEmail(Email email)
        {
            Email = email;
        }

        public void SetAccountStatus(AccountStatus accountStatus)
        {
            AccountStatus = accountStatus;
            AccountStatusCode = accountStatus.Id;
        }

        public void SetConcurrencyStamp(string concurrencyStamp)
        {
            ConcurrencyStamp = concurrencyStamp;
        }

        public void SetTwoFactorEnabled(bool twoFactorEnabled)
        {
            TwoFactorEnabled = twoFactorEnabled;
        }

        public void SetLockoutEnabled(bool lockoutEnabled)
        {
            LockoutEnabled = lockoutEnabled;
        }

        public void SetLockoutEnd(DateTimeOffset? lockoutEnd)
        {
            LockoutEnd = lockoutEnd;
        }

        public void SetAccessFailedCount(int accessFailedCount)
        {
            AccessFailedCount = accessFailedCount;
        }

        public void SetSecurityStamp(string securityStamp)
        {
            SecurityStamp = securityStamp;
        }

        public void SetTimezone(string timezone)
        {
            Timezone = timezone;
        }

        public void SetIsResetMFA(bool isResetMFA)
        {
            IsResetMFA = isResetMFA;
        }
    }
}
