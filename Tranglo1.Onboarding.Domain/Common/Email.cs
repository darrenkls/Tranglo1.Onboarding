using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Tranglo1.Onboarding.Domain.Common
{
    public class Email : ValueObject
    {
        public Email() : base() { }

        public const int MaxLength = 320;
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Result<Email> Create(string value)
        {
            value = value?.Trim();

            if (string.IsNullOrEmpty(value))
                return Result.Failure<Email>("Email cannot be blank");

            if (value.Length > MaxLength)
                return Result.Failure<Email>($"Email cannot be longer than {MaxLength} characters.");

            try
            {
                _ = new MailAddress(value);
            }
            catch (FormatException ex)
            {
                return Result.Failure<Email>(ex.Message);
            }

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
