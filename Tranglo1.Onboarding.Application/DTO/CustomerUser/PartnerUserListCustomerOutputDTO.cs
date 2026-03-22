namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class PartnerUserListCustomerOutputDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserRole { get; set; }
        public string Email { get; set; }
        public string UserEnvironment { get; set; }
        public long CompanyUserBlockStatusCode { get; set; }
        public string BlockStatus { get; set; }
        public long CompanyUserAccountStatusCode { get; set; }
        public string AccountStatus { get; set; }
        public string RoleCode { get; set; }
    }
}
