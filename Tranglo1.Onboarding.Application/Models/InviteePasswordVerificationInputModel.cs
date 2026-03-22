using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.Models
{
    public class InviteePasswordVerificationInputModel : IValidatableObject
    {
        [StringLength(200, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(Constants.PasswordRegex, ErrorMessage = Constants.InvalidPasswordMessage)]
        public string NewPassword { get; set; }

        public string RecaptchaToken { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
