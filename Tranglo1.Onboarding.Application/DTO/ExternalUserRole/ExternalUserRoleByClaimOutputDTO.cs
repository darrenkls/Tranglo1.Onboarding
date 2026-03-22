using System;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.ExternalUserRole
{
    public class ExternalUserRoleByClaimOutputDTO
    {
        public string Timezone { get; set; }
        public List<UserSolution> UserSolution { get; set; }
    }
    public class UserSolution
    {
        public long SolutionCode { get; set; }
        public string SolutionDescription { get; set; }
        public List<ExternalUser> ExternalUser { get; set; }
    }
    public class ExternalUser
    {
        public int BusinessProfileCode { get; set; }
        public int PartnerCode { get; set; }
        public bool isDisabled { get; set; }
        public string CompanyName { get; set; }
        public int UserAccountStatusCode { get; set; }
        public int BlockStatusCode { get; set; }
        public int CompanyAccountStatusCode { get; set; }
        public long SolutionCode { get; set; }
        public long CustomerTypeCode { get; set; }
        public string CustomerTypeDescription { get; set; }
        public long PartnerTypeCode { get; set; }
        public string PartnerTypeDescription { get; set; }
        public long BusinessDeclarationStatusCode { get; set; }
        public string BusinessDeclarationStatusDescription { get; set; }
        public long? CustomerVerificationCode { get; set; }
        public long? EKYCVerificationStatusCode { get; set; }
        public string EKYCVerificationStatusDescription { get; set; }
        public long? F2FVerificationStatusCode { get; set; }
        public string F2FVerificationStatusDescription { get; set; }
        public List<ExternalUserRoles> ExternalUserRoles { get; set; }
        public int TrangloEntityCode { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int PartnerAccountStatusCode { get; set; }
        public string PartnerAccountStatusDescription { get; set; }
    }
    public class ExternalUserRoles
    {
        public long BusinessProfileCode { get; set; }
        public long SolutionCode { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
    }
}
