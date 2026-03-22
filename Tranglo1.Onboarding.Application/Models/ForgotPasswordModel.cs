using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
