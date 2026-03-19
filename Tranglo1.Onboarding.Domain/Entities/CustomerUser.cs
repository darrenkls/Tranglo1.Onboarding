using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerUser : ApplicationUser
    {
        protected CustomerUser() { }

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

        public void ConfirmInviteeEmail()
        {
            if (EmailConfirmed == false)
            {
                EmailConfirmed = true;
                SetAccountStatus(AccountStatus.Active);
            }
        }
    }
}
