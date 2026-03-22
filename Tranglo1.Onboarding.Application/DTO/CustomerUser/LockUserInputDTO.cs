using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.DTO.CustomerUser
{
    public class LockUserInputDTO
    {
        [Required]
        public string Email { get; set; }
    }
}
