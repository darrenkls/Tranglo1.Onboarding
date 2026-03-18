using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ScreeningTypeList : Enumeration
    {
        public ScreeningTypeList(int id, string name) : base(id, name)
        {
        }

        public ScreeningTypeList()
        {
        }

        public static readonly ScreeningTypeList PEP = new ScreeningTypeList(1, "PEP");
        public static readonly ScreeningTypeList Sanctions = new ScreeningTypeList(2, "Sanctions");
        public static readonly ScreeningTypeList PEPAndSanctions = new ScreeningTypeList(3, "PEP & Sanctions");
        public static readonly ScreeningTypeList SOE = new ScreeningTypeList(4, "SOE");
        public static readonly ScreeningTypeList Adverse_Media = new ScreeningTypeList(5, "Adverse Media");
    }
}
