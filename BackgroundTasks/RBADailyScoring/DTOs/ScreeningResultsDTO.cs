namespace Tranglo1.RBADailyScoring.DTOs
{
    public class ScreeningResultsDTO
    {
        public long OwnershipStrucureType { get; set; }
        public bool IsPEP { get; set; }
        public bool IsAdverseMedia { get; set; }
        public decimal PercentageOfOwnership { get; set; }
        public bool IsWatchList { get; set; }
        public int? WatchlistStatus { get; set; }
    }
}
