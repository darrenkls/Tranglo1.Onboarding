using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.PartnerManagement
{
    public class PartnerAgreementStatus : Enumeration
    {
        public PartnerAgreementStatus() : base()
        {

        }

        public PartnerAgreementStatus(int id, string name) : base(id, name)
        {

        }

        public static readonly PartnerAgreementStatus Active = new PartnerAgreementStatus(1, "Active");
        public static readonly PartnerAgreementStatus Expired = new PartnerAgreementStatus(2, "Expired");
        public static readonly PartnerAgreementStatus SigningInProgress = new PartnerAgreementStatus(3, "Signing In Progress");
    }
}
