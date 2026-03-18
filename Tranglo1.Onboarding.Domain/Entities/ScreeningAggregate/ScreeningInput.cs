using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class ScreeningInput : AggregateRoot
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public short? YearOfBirth { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? LastReviewedDate { get; set; }
        public long TableId { get; set; }
        public int PEPCount { set; get; }
        public int SanctionCount { set; get; }
        public int SOECount { set; get; }
        public int AdverseMediaCount { set; get; }
        public int EnforcementCount { get; set; }
        public int AssociatedEntityCount { get; set; }

        [ForeignKey(nameof(OwnershipStrucureType))]
        public long? OwnershipStrucureTypeId { get; set; }

        [ForeignKey(nameof(ScreeningEntityType))]
        public long? ScreeningEntityTypeId { get; set; }

        [ForeignKey(nameof(BusinessProfile))]
        public int? BusinessProfileId { get; set; }

        [ForeignKey(nameof(NationalityMeta))]
        public long? NationalityCountryCode { get; set; }

        [ForeignKey(nameof(WatchlistStatus))]
        public long? WatchlistStatusId { get; set; }

        [ForeignKey(nameof(ScreeningInputEnforcementAction))]
        public long? ScreeningInputEnforcementActionCode { get; set; }

        #region Navigation Properties
        public OwnershipStrucureType OwnershipStrucureType { get; set; }    //bod, share holder, legal entities
        public ScreeningEntityType ScreeningEntityType { set; get; }
        public BusinessProfile BusinessProfile { set; get; }
        public CountryMeta NationalityMeta { get; set; }
        public WatchlistStatus WatchlistStatus { set; get; }
        public EnforcementActions ScreeningInputEnforcementAction { get; set; }
        public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
        #endregion Navigation Properties


        public ScreeningInput()
        {

        }

        public ScreeningInput(ScreeningEntityType screeningEntityType, BusinessProfile businessProfile, 
            OwnershipStrucureType ownershipStrucureType, long tableId,
            string fullName, DateTime? dateOfBirth,
            CountryMeta nationalityMeta, string gender)
        {
            ScreeningEntityTypeId = screeningEntityType.Id;
            BusinessProfileId = businessProfile.Id;
            OwnershipStrucureTypeId = ownershipStrucureType.Id;
            TableId = tableId;
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            NationalityCountryCode = nationalityMeta?.Id;
            Gender = gender;
        }

        /// <summary>
        /// Get ScreeningEntityType (navigation property) by foreign key when it's not loaded
        /// </summary>
        /// <returns></returns>
        public ScreeningEntityType GetScreeningEntityType()
        {
            if (ScreeningEntityTypeId == null)
            {
                return null;
            }
            return Enumeration.FindById<ScreeningEntityType>(ScreeningEntityTypeId.Value);
        }

        /// <summary>
        /// Get NationalityMeta (navigation property) by foreign key when it's not loaded.
        /// </summary>
        /// <returns></returns>
        public CountryMeta GetNationalityMeta()
        {
            if (NationalityCountryCode == null)
            {
                return null;
            }
            return Enumeration.FindById<CountryMeta>(NationalityCountryCode.Value);
        }

        public OwnershipStrucureType GetOwnershipStrucureType()
        {
            if (OwnershipStrucureTypeId == null)
            {
                return null;
            }
            return Enumeration.FindById<OwnershipStrucureType>(OwnershipStrucureTypeId.Value);
        }

        public void ReviewScreeningResult(WatchlistStatus watchlistStatus)
        {
            LastReviewedDate = DateTime.UtcNow;

            if (WatchlistStatus != watchlistStatus)
            {
                WatchlistStatus = watchlistStatus;
            }
        }

        public void UpsertScreeningResult(NameScreenerResponse nameScreenerResponse, IEnumerable<GetEntityDetailByReferenceCodeResponse> getEntityDetailResponses)
        {
            PEPCount = nameScreenerResponse.Summary.PEP;
            SanctionCount = nameScreenerResponse.Summary.SanctionList;
            SOECount = nameScreenerResponse.Summary.SOE;
            AdverseMediaCount = nameScreenerResponse.Summary.AdverseMedia;
            EnforcementCount = nameScreenerResponse.Summary.Enforcement;
            AssociatedEntityCount = nameScreenerResponse.Summary.AssociatedEntity;
            ScreeningInputEnforcementActionCode = AdverseMediaCount > 0 ? EnforcementActions.NA.Id : EnforcementActions.NoBySystem.Id;

            var newScreening = new Screening
            {
                ReferenceCode = nameScreenerResponse.Reference,
                PEPCount = nameScreenerResponse.Summary.PEP,
                SanctionCount = nameScreenerResponse.Summary.SanctionList,
                SOECount = nameScreenerResponse.Summary.SOE,
                AdverseMediaCount = nameScreenerResponse.Summary.AdverseMedia,
                EnforcementCount = nameScreenerResponse.Summary.Enforcement,
                AssociatedEntityCount = nameScreenerResponse.Summary.AssociatedEntity,
                ScreeningDate = DateTime.UtcNow
            };

            Screenings.Add(newScreening);
            newScreening.AddScreeningDetails(getEntityDetailResponses);
        }

        public void UpdateWatchlistStatus(WatchlistStatus watchlistStatus)
        {
            WatchlistStatusId = watchlistStatus?.Id;
        }
    }
}
