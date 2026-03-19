namespace Tranglo1.Onboarding.Domain.Common
{
    /// <summary>
    /// Stub — proper definition belongs in Tranglo1.Identity.Contracts NuGet package.
    /// Replace with Contracts type once the package is updated.
    /// </summary>
    public class Email
    {
        public string Value { get; }

        public Email(string value) { Value = value; }

        public static implicit operator string(Email email) => email?.Value;

        public override string ToString() => Value;

        public override bool Equals(object obj) =>
            obj is Email other && Value == other.Value;

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    }
}
