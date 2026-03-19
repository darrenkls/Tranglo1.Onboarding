using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tranglo1.Onboarding.Domain.Common
{
    public class FullName : ValueObject
    {
        public const int MaxLength = 300;
        public string Value { get; }

        private FullName(string value)
        {
            Value = value;
        }

        public static Result<FullName> Create(string value)
        {
            value = value?.Trim();

            if (!string.IsNullOrEmpty(value))
                value = Regex.Replace(value, @"\s+", " ");

            if (string.IsNullOrEmpty(value))
                return Result.Failure<FullName>("Full Name cannot be blank");

            if (value.Length > MaxLength)
                return Result.Failure<FullName>($"Full Name cannot be longer than {MaxLength} characters.");

            return Result.Success(new FullName(value));
        }

        public static implicit operator string(FullName fullName) => fullName?.Value;

        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
