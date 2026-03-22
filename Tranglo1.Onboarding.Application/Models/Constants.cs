namespace Tranglo1.Onboarding.Application.Models
{
    internal class Constants
    {
        public const string MobileNumberRegex = @"^[0-9]+$";
        public const string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string CountryCodeIso2Regex = @"[a-zA-Z]{2}";
        public const string PasswordRegex = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-.`()_=]).{8,}$";
        public const string InvalidPasswordMessage = "Password must contain at least 8 characters, 1 lower case, 1 upper case, 1 numeric and 1 symbol";

        public const string AlphaNumericRegex = @"^[a-zA-Z0-9]*$";
        public const string InvalidAlphaNumericMessage = "Only alphanumeric is allowed.";
        public const string ContactNumberRegex = @"^[0-9]*$";
        public const string InvalidContactNumberMessage = "Contact Number format is incorrect. Only allow digit and dash";
        public const string NumbersOnlyRegex = @"^[0-9]+$";
        public const string InvalidNumbersOnlyMessage = "ONLY numeric is allowed";
    }
}
