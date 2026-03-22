using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CompanyName : ValueObject
    {
        public CompanyName() : base() { }

        public const int MaxLength = 150;
        public string Value { get; }

        private CompanyName(string value)
        {
            Value = value;
        }

        public static Result<CompanyName> Create(string value)
        {
            value = value?.Trim();
            value = Regex.Replace(value, @"\s+", " ");

            if (value.Length > MaxLength)
                return Result.Failure<CompanyName>($"Company Name cannot be longer than {MaxLength} characters.");

            return Result.Success(new CompanyName(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
