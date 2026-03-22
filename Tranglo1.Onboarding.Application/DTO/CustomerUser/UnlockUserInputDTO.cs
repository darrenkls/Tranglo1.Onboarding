using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class UnlockUserInputDTO
    {
        [Required]
        public string Email { get; set; }
    }
}
