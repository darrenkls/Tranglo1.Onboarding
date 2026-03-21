namespace Tranglo1.Onboarding.Domain.Common.Extensions
{
    public static partial class Extensions
    {
        /// <summary>
        /// Returns the original string, or a placeholder if the string is null, empty, or whitespace.
        /// </summary>
        public static string ToEmptyPlaceholder(this string value, string emptyPlaceholder)
        {
            return string.IsNullOrWhiteSpace(value) ? emptyPlaceholder : value;
        }
    }
}
