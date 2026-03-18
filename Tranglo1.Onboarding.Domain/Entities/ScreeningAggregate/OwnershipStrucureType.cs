using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate
{
    public class OwnershipStrucureType : Enumeration
    {
        public OwnershipStrucureType()
        {
        }

        public OwnershipStrucureType(int id, string name) : base(id, name)
        {
        }

        public static readonly OwnershipStrucureType BusinessProfile = new OwnershipStrucureType(1, "Business Profile");
        public static readonly OwnershipStrucureType CompanyShareholder = new OwnershipStrucureType(2, "Company Shareholder");
        public static readonly OwnershipStrucureType IndividualShareholder = new OwnershipStrucureType(3, "Individual Shareholder");
        public static readonly OwnershipStrucureType IndividualLegalEntity = new OwnershipStrucureType(5, "Individual UBO");
        public static readonly OwnershipStrucureType BoardOfDirector = new OwnershipStrucureType(7, "Board of Director");
        public static readonly OwnershipStrucureType PrimaryOfficer = new OwnershipStrucureType(8, "Principal Officer");
        public static readonly OwnershipStrucureType AffiliatesAndSubsidiaries = new OwnershipStrucureType(10, "Licensed Parent Company, Subsidiaries & Affiliates");
        public static readonly OwnershipStrucureType AuthorisedPerson = new OwnershipStrucureType(11, "Authorised Person");
        public static readonly OwnershipStrucureType ShareholderIndividualLegalEntity = new OwnershipStrucureType(12, "Individual Ultimate Shareholder");
        public static readonly OwnershipStrucureType ShareholderCompanyLegalEntity = new OwnershipStrucureType(13, "Company Ultimate Shareholder");

        /// <summary>
        /// The following concept are no longer in use
        /// </summary>
        public static readonly OwnershipStrucureType CompanyLegalEntity = new OwnershipStrucureType(4, "CompanyLegalEntity");
        public static readonly OwnershipStrucureType ParentHoldings = new OwnershipStrucureType(6, "ParentHoldings");
        public static readonly OwnershipStrucureType PoliticalExposedPersons = new OwnershipStrucureType(9, "PoliticalExposedPersons");

        public static List<OwnershipStrucureType> GetObsoletedOwnershipStructureTypes()
        {
            return new List<OwnershipStrucureType>
            {
                CompanyLegalEntity,
                ParentHoldings,
                PoliticalExposedPersons
            };
        }
    }
}
