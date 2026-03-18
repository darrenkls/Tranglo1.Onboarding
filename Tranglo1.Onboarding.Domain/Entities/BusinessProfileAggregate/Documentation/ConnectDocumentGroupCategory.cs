using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.Documentation
{
    public class ConnectDocumentGroupCategory : Enumeration
    {
        public ConnectDocumentGroupCategory() : base()
        {
        }

        public ConnectDocumentGroupCategory(int id, string name)
            : base(id, name)
        {

        }

        public static readonly ConnectDocumentGroupCategory BusinessProfile = new ConnectDocumentGroupCategory(1, "Business Profile");
        public static readonly ConnectDocumentGroupCategory LicenseInformation = new ConnectDocumentGroupCategory(2, "License Information");
        public static readonly ConnectDocumentGroupCategory OrganisationalStructure = new ConnectDocumentGroupCategory(3, "Organisational Structure");
        public static readonly ConnectDocumentGroupCategory Other = new ConnectDocumentGroupCategory(4, "Others");
    }
}
