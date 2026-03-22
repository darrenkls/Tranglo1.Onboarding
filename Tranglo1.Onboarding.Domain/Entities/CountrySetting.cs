using CSharpFunctionalExtensions;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class CountrySetting : Entity<int>
    {
        public CountryMeta Country { get; set; }
        public bool IsHighRisk { get; set; }
        public bool IsSanction { get; set; }
        public bool IsDisplay { get; set; }
        public bool IsRejectTransaction { get; set; }
    }
}
