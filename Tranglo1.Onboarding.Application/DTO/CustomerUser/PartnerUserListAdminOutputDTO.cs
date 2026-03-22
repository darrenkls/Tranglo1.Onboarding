using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class UserRoleSolution
    {
        public string UserRole { get; set; }
        public string Solution { get; set; }
    }

    public class PartnerUserListAdminOutputDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public int BusinessProfileCode { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string UserEnvironment { get; set; }
        public string BlockStatus { get; set; }
        public string AccountStatus { get; set; }
        public int AccountStatusCode { get; set; }
        public long CustomerUserRegistrationId { get; set; }
        public long CustomerUserBusinessProfileCode { get; set; }
        public List<UserRoleSolution> UserRoles { get; set; } = new List<UserRoleSolution>();
    }
}
