using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.Models
{
    public class RegisterWithRegistryCodeInputModel : IValidatableObject
    {
        [Required(ErrorMessage = "Registry Code is required")]
        [MaxLength(150)]
        [Display(Name = "RegistryCode")]
        public string RegistryCode { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(150)]
        [Display(Name = "Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [MaxLength(150)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [StringLength(128, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least 8 characters, 1 lower case, 1 upper case, 1 numeric and 1 symbol")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms and Conditions!")]
        public bool IsTermsAndConditionReadAgreed { get; set; }

        public string RecaptchaToken { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
