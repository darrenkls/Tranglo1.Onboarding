using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class ViewPartnerUserAdminOutputDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int UserEnvironmentCode { get; set; }
        public string UserEnvironment { get; set; }
        public int AccountStatusCode { get; set; }
        public string AccountStatus { get; set; }
        public string Timezone { get; set; }
        public long CustomerUserBusinessProfileCode { get; set; }
        public List<BusinessProfileRoles> BusinessProfileRole { get; set; } = new List<BusinessProfileRoles>();

        public class BusinessProfileRoles
        {
            public int CompanyCode { get; set; }
            public string Company { get; set; }
            public string UserRoleCode { get; set; }
            public string UserRole { get; set; }
            public long SolutionCode { get; set; }
            public string Solution { get; set; }
        }
    }
}
