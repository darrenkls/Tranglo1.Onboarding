using System.Collections.Generic;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class UpdatePartnerUserCustomerInputDTO
    {
        public string Name { get; set; }
        public List<UserRolesInputDTO> UserRole { get; set; }
        public string Timezone { get; set; }
    }
}
