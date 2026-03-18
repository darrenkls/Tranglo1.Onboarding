using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.ExternalUserRoleAggregate;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CustomerUserBusinessProfileRole : Entity
    {
        public CustomerUserBusinessProfile CustomerUserBusinessProfile { get; private set; }
        public long CustomerUserBusinessProfileCode { get; set; }
        public UserRole UserRole { get; private set; } //To remove
        public long? UserRoleCode { get; set; } //To remove
        public string RoleCode { get; private set; }

        private CustomerUserBusinessProfileRole()
        {

        }

        public CustomerUserBusinessProfileRole(CustomerUserBusinessProfile customerUserBusinessProfile, ExternalUserRole role)
        {
            this.CustomerUserBusinessProfile = customerUserBusinessProfile;
            this.CustomerUserBusinessProfileCode = customerUserBusinessProfile.Id;

            this.RoleCode = role?.RoleCode;
        }
    }
}
