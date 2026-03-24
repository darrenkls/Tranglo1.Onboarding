using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class ResendInvitationInputDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [MaxLength(150)]
        [DataType(DataType.EmailAddress)]
        public string InviteeEmail { get; set; }
    }
}
