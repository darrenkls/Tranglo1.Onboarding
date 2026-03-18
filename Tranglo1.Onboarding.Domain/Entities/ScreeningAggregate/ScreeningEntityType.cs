using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ScreeningEntityType : Enumeration
    {
        public ScreeningEntityType(int id, string name, string externalDescription) : base(id, name)
        {
            ExternalDescription = externalDescription;
        }

        public ScreeningEntityType()
        {
        }

        public string ExternalDescription { get; set; }

        public static readonly ScreeningEntityType Individual = new ScreeningEntityType(1, "Individual", "Individual");
        public static readonly ScreeningEntityType Natural = new ScreeningEntityType(2, "Natural", "Corporate"); //For Corporate Entities in  the External LN Screening uses Natural
    }
}
