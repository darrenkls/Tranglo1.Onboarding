using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class Shareholder: Entity
    {
        public DateTime? DateOfIncorporation { get; set; }
        public BusinessProfile BusinessProfile { get; private set; }
        public string EffectiveShareholding { get; set; }
        public BoardOfDirector BoardOfDirector { get; set; }
        public PrimaryOfficer PrimaryOfficer { get; set; }
        public AuthorisedPerson AuthorisedPerson { get; set; }
        public IndividualLegalEntity UltimateBeneficialOwner { get; set; }
        public bool IsQuarterlyOwned { get; set; }


        private protected Shareholder()
        {

        }

        public Shareholder(BusinessProfile businessProfile, DateTime? dateOfIncorporation, string effectiveShareholding,
                            BoardOfDirector boardOfDirector, PrimaryOfficer primaryOfficer,AuthorisedPerson authorisedPerson, IndividualLegalEntity ultimateBeneficialOwner)
        {
            this.BusinessProfile = businessProfile;
            this.DateOfIncorporation = dateOfIncorporation;
            this.EffectiveShareholding = effectiveShareholding;
            this.BoardOfDirector = boardOfDirector;
            this.PrimaryOfficer = primaryOfficer;
            this.AuthorisedPerson = authorisedPerson;
            this.UltimateBeneficialOwner = ultimateBeneficialOwner;
        }
    }
}
