using CSharpFunctionalExtensions;
using System.Collections.Generic;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ContactNumber : ValueObject
    {
        public ContactNumber() : base() { }

        public const int MaxLength = 50;
        public string CountryISO2Code { get; }
        public string DialCode { get; }
        public string Value { get; }

        private ContactNumber(string dialCode, string countryISO2Code, string value)
        {
            DialCode = dialCode;
            Value = value;
            CountryISO2Code = countryISO2Code;
        }

        public static Result<ContactNumber> Create(string dialCode, string countryISO2Code, string value)
        {
            dialCode = dialCode?.Trim();
            value = value?.Trim();

            return Result.Success(new ContactNumber(dialCode, countryISO2Code, value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DialCode + Value;
        }
    }
}
