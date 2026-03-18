using System;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCWatchListReviewOutputDTO
    {
        public string CompanyName { get; set; }
        public string OwnershipCategory { get; set; }
        public string EntityType { get; set; }
        public string CountryISO2 { get; set; }
        public string CountryDescription { get; set; }
        public string FullName { get; set; }
        public DateTime ScreeningDate { get; set; }
        public bool IsPEP { get; set; }
        public bool IsSanction { get; set; }
        public bool IsSOE { get; set; }
        public bool IsAdverseMedia { get; set; }
        public bool IsEnforcement { get; set; }
        public string WatchlistStatus { get; set; }
        public string ComplianceOfficer { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public bool? IsTrueHitPEP { get; set; }
        public bool? IsTrueHitSanction { get; set; }
        public bool? IsTrueHitSOE { get; set; }        
        public bool? IsTrueHitAdverseMedia { get; set; }
        public bool? IsTrueHitEnforcement { get; set; }
        public int ScreeningInputCode { get; set; }
        public long? EnforcementActionsCode { get; set; }
        public string EnforcementActionsDescription { get; set; }
    }
}
