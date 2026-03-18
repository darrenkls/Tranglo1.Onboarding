using System;
using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses
{
    public class GetEntityDetailByReferenceCodeResponse
    {
        public int EntityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string MiddleName { get; set; }
        public string EntryType { get; set; }
        public string EntryCategory { get; set; }   // AdverseMedia, AssociatedEntity, Enforcement, PEP, Registrations SanctionList, SOE
        public string Comments { get; set; }
        public string DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Country { get; set; }
        public DateTime? ListDate { get; set; }
        public bool? HasNegativeNews { get; set; }
        public string PrimaryName { get; set; }
        public string Gender { get; set; }  // Male, Female
        public int? ListSource { get; set; }
        public string ListSourceName
        {
            get
            {
                if (!this.ListSource.HasValue)
                {
                    return string.Empty;
                }
                var listSource = Enumeration.FindById<ScreeningListSource>(this.ListSource.Value);
                return listSource?.Name ?? string.Empty;
            }
        }
        public string Nationality { get; set; } // ISO2 Country Code
        public int? ParentId { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Aka { get; set; }
        public string NameSource { get; set; }
        public string GovDesignation { get; set; }
        public string EntrySubCategory { get; set; }
        public string Organization { get; set; }
        public string Positions { get; set; }
        public List<string> SourceWebLinks { get; set; }
        public string OriginalName { get; set; }

        public ScreeningDetailsCategory GetScreeningDetailCategory()
        {
            if (IsCategorizedAsSanction())
            {
                return ScreeningDetailsCategory.Sanctions;
            }

            if (string.Equals(EntryCategory, "PEP", StringComparison.OrdinalIgnoreCase))
            {
                return ScreeningDetailsCategory.PEP;
            }

            if (string.Equals(EntryCategory, "SOE", StringComparison.OrdinalIgnoreCase))
            {
                return ScreeningDetailsCategory.SOE;
            }

            if (string.Equals(EntryCategory, "AdverseMedia", StringComparison.OrdinalIgnoreCase))
            {
                return ScreeningDetailsCategory.Adverse_Media;
            }

            if (string.Equals(EntryCategory, "Enforcement", StringComparison.OrdinalIgnoreCase))
            {
                return ScreeningDetailsCategory.Enforcement;
            }

            return null;
        }

        public Gender GetGender()
        {
            if (string.Equals(Gender, "Male", StringComparison.OrdinalIgnoreCase))
            {
                return Entities.Gender.Male;
            }

            if (string.Equals(Gender, "Female", StringComparison.OrdinalIgnoreCase))
            {
                return Entities.Gender.Female;
            }

            return null;
        }

        #region Private Helper Methods
        /// <summary>
        /// The entities will be categorized as Sanctions based on multiple conditions
        /// 1. Category is "SanctionList" or "AssociatedEntity"
        /// 2. Category is "Enforcement" and SubCategory is one of "Terrorism", "WMD", "End Use Control"
        /// </summary>
        /// <returns></returns>
        private bool IsCategorizedAsSanction()
        {
            return string.Equals(EntryCategory, "SanctionList", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(EntryCategory, "AssociatedEntity", StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(EntryCategory, "Enforcement", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(EntrySubCategory, "Terrorism", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(EntrySubCategory, "WMD", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(EntrySubCategory, "End Use Control", StringComparison.OrdinalIgnoreCase)));
        }
        #endregion Private Helper Methods
    }
}
