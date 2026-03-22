using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class IncorporationCompanyType : Enumeration
    {
        public IncorporationCompanyType() : base() { }

        public IncorporationCompanyType(int id, string name) : base(id, name) { }

        public static readonly IncorporationCompanyType Public_Listed_Company = new IncorporationCompanyType(1, "Public Listed Company");
        public static readonly IncorporationCompanyType Limited_Company = new IncorporationCompanyType(2, "Limited Company");
        public static readonly IncorporationCompanyType Private_Limited_Company = new IncorporationCompanyType(3, "Private Limited Company");
        public static readonly IncorporationCompanyType Partnership = new IncorporationCompanyType(4, "Partnership");
        public static readonly IncorporationCompanyType Sole_Proprietorship = new IncorporationCompanyType(5, "Sole Proprietorship");
        public static readonly IncorporationCompanyType Association_Club_NGO = new IncorporationCompanyType(6, "Association / Club / NGO");
        public static readonly IncorporationCompanyType Foundation_Trust = new IncorporationCompanyType(7, "Foundation / Trust");
    }
}
