using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.Models
{
    public class LoginInputModel : IValidatableObject
    {
        [Required]
        [ModelBinder(Name = "ux_user")]
        public string Username { get; set; }

        [Required]
        [ModelBinder(Name = "ux_pass")]
        public string Password { get; set; }

        public string Country { get; set; }
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
        public string RecaptchaToken { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}
