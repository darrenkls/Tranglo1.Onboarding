using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement
{
    public class KYCWatchlistDetailsHistoryOutputDTO
    {
        public int WatchlistCode { get; set; }
        public string LastReviewedBy { get; set; }
        public DateTime LastReviewedDate { get; set; }
        public bool IsTrueHitSanction { get; set; }
        public bool IsTrueHitSOE { get; set; }
        public bool IsTrueHitPEP { get; set; }
        public bool? IsTrueHitAdverseMedia { get; set; }
        public bool IsTrueHitEnforcement { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public Guid[] DocumentId { get; set; }
        public long? EnforcementActionsCode { get; set; }
        public string EnforcementActionsDescription { get; set; }
    }
}
