using System.ComponentModel.DataAnnotations;

namespace Tranglo1.Onboarding.Application.Models
{
    public class CreatePasswordModel
    {
        public string Email { get; set; }
        public string Token { get; set; }

        [DataType(DataType.Password)]
        [RegularExpression(Constants.PasswordRegex, ErrorMessage = Constants.InvalidPasswordMessage)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Confirm password doesn't match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
