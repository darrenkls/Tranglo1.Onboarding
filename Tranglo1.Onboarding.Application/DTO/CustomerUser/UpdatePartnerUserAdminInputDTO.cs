using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class UpdatePartnerUserAdminInputDTO
    {
        public string Name { get; set; }
        public List<CompanyRoleInputDTO> CompanyRole { get; set; }
        public string Timezone { get; set; }
    }
}
