using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Partner.PartnerSubscription
{
    public class SubscriptionEntityRolesOutputDTO
    {
        public List<UserEntity> UserEntities { get; set; }
    }

    public class UserEntity
    {
        public string Entity { get; set; }
        public string EntityDescription { get; set; }
        public int UserAccountStatusCode { get; set; }
        public int BlockStatusCode { get; set; }
        public List<UserEntityRole> UserEntityRoles { get; set; }
    }

    public class UserEntityRole
    {
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public int AuthorityLevelCode { get; set; }
        public bool IsSuperApprover { get; set; }
    }
}