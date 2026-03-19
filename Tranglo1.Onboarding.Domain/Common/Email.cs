using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tranglo1.Onboarding.Domain.Common
{
    public class Email : ValueObject
    {
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Result<Email> Create(string value)
        {
            value = value?.Trim().ToLower();

            if (string.IsNullOrEmpty(value))
                return Result.Failure<Email>("Email cannot be blank");

            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Result.Failure<Email>("Email format is invalid");

            return Result.Success(new Email(value));
        }

        public static implicit operator string(Email email) => email?.Value;

        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
