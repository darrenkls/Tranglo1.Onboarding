using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class @string : Enumeration
    {
        public @string() : base()
        {
        }

        public @string(int id, string name)
            : base(id, name)
        {
        }

        public static readonly @string Malaysia_Ringgit = new @string(1, "Malaysia Ringgit(MYR)");
        public static readonly @string Singapore_Dollar = new @string(2, "Singapore Dollar(SGD)");
        public static readonly @string US_Dollar = new @string(3, "U.S. Dollar(USD)");
        public static readonly @string European_Euro = new @string(4, "European Euro(EUR)");
        public static readonly @string Indonesia_Rupiah = new @string(5, "Indonesia Rupiah(IDR)");
        public static readonly @string Hong_Kong_Dollar = new @string(6, "Hong Kong Dollar(HKD)");
    }
}
