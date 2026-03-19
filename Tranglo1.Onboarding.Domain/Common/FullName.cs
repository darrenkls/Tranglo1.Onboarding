namespace Tranglo1.Onboarding.Domain.Common
{
    /// <summary>
    /// Stub — proper definition belongs in Tranglo1.Identity.Contracts NuGet package.
    /// Replace with Contracts type once the package is updated.
    /// </summary>
    public class FullName
    {
        public string Value { get; }

        public FullName(string value) { Value = value; }

        public static implicit operator string(FullName fullName) => fullName?.Value;

        public override string ToString() => Value;
    }
}
