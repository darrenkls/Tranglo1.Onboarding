using System;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Events;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerUser : ApplicationUser
    {
        protected CustomerUser() : base() { }

        public virtual bool EmailConfirmed { get; protected set; }
        public virtual string PasswordHash { get; set; }
        public CountryMeta CountryMeta { get; protected set; }
        public CompanyUserBlockStatus CompanyUserBlockStatus { get; set; }
        public virtual bool IsTPNUser { get; protected set; }

        public override void SetEmail(Email email)
        {
            base.SetEmail(email);
            LoginId = email.Value;
        }

        public void setIsTPNUser(bool value)
        {
            IsTPNUser = value;
        }

        public void ConfirmInviteeEmail()
        {
            if (EmailConfirmed == false)
            {
                EmailConfirmed = true;
                SetAccountStatus(AccountStatus.Active);
            }
        }

        public void ConfirmEmail(int? solutionCode, bool isMultipleSolution)
        {
            if (EmailConfirmed == false)
            {
                ConfirmInviteeEmail();
                base.AddDomainEvent(new CustomerUserEmailVerifiedEvent(Email.Value, solutionCode, isMultipleSolution));
            }
        }

        internal static CustomerUser Create(FullName fullName, Email email, string passwordHash, CountryMeta countryMeta, string timezone)
        {
            var customer = new CustomerUser
            {
                PasswordHash = passwordHash,
                CountryMeta = countryMeta,
                AccountStatusCode = AccountStatus.PendingActivation.Id
            };

            customer.SetEmail(email);
            customer.SetName(fullName);
            customer.SetTimezone(timezone);

            return customer;
        }

        public static CustomerUser Register(FullName fullName, Email email, string passwordHash, CountryMeta countryMeta, int? solutionCode, bool isMultipleSolutions)
        {
            var customerUser = Create(fullName, email, passwordHash, countryMeta, null);

            customerUser.AddDomainEvent(new CustomerUserRegisteredEvent(email.Value, fullName.Value, DateTime.UtcNow, solutionCode, isMultipleSolutions));

            return customerUser;
        }
    }
}
