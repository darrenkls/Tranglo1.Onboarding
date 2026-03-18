
namespace Tranglo1.RBADailyScoring.DTOs
{
    public class WatchlistReviewDTO
    {
        public bool? IsTrueHitPEP { set; get; }
        public bool? IsTrueHitSanction { set; get; }
        public bool? IsTrueHitSOE { set; get; }
        public bool? IsTrueHitAdverseMedia { set; get; }
        public long WatchlistStatusCode { set; get; }
        public string Remarks { set; get; }
        public long EnforcementActionsCode { set; get; }
    }
}
