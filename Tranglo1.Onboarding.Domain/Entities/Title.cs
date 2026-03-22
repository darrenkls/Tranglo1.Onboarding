using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Title : Enumeration
    {
        public Title() : base() { }

        public Title(int id, string name) : base(id, name) { }

        public static readonly Title Mr = new Title(1, "Mr");
        public static readonly Title Mrs = new Title(2, "Mrs");
        public static readonly Title Ms = new Title(3, "Ms");
        public static readonly Title Mdm = new Title(4, "Mdm");
        public static readonly Title Dr = new Title(5, "Dr");
        public static readonly Title Prof = new Title(6, "Prof");
        public static readonly Title Sir = new Title(7, "Sir");
        public static readonly Title Dato = new Title(8, "Dato");
        public static readonly Title Datin = new Title(9, "Datin");
        public static readonly Title Tan_Sri = new Title(10, "Tan Sri");
        public static readonly Title Tun = new Title(11, "Tun");
        public static readonly Title Tunku = new Title(12, "Tunku");
        public static readonly Title Haji = new Title(13, "Haji");
        public static readonly Title Hajah = new Title(14, "Hajah");
        public static readonly Title Others = new Title(15, "Others");
    }
}
