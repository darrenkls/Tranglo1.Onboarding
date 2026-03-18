namespace Tranglo1.RBADailyScoring.DTOs
{
    public class ScreeningInputsDTO
    {
        public int ScreeningInputCode { get; set; }
        public int OwnershipStrucureTypeId { get; set; }
        public long TableId { get; set; }
        public int? WatchlistStatus { set; get; }
        public int PEPCount { set; get; }
        public int SanctionCount { set; get; }
        public int SOECount { set; get; }
        public int AdverseMediaCount { set; get; }
    }
}
