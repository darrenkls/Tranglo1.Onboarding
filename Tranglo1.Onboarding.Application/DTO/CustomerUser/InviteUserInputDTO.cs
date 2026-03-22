using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class InviteUserInputDTO
    {
        public int UserEnvironmentCode { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(150)]
        public string InviteeFullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(150)]
        [DataType(DataType.EmailAddress)]
        public string InviteeEmail { get; set; }

        [Required(ErrorMessage = "Invitee role code is required")]
        public List<string> InviteeRoleCodeList { get; set; }

        public string Timezone { get; set; }
    }
}
