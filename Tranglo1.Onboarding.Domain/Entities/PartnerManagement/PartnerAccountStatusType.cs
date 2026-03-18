using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class PartnerAccountStatusType : Enumeration
    {
        public PartnerAccountStatusType() : base()
        {
        }

        public PartnerAccountStatusType(int id, string name)
            : base(id, name)
        {

        }

        public static readonly PartnerAccountStatusType Active = new PartnerAccountStatusType(1, "Active");
        public static readonly PartnerAccountStatusType Inactive = new PartnerAccountStatusType(2, "Inactive");
        public static readonly PartnerAccountStatusType Rejected = new PartnerAccountStatusType(3, "Rejected");
    }
}
