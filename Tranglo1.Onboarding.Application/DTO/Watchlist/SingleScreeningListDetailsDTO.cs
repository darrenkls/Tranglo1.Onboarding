using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class SingleScreeningListDetailsDTO
    {
        public long EntityId { set; get; }
        public DateTime ListDate { set; get; }
        public string EntryCategory { set; get; }
        public string EntrySubCategory { set; get; }
        public bool? IsTrueHitPEP { set; get; }
        public bool? IsTrueHitSanction { set; get; }
        public bool? IsTrueHitSOE { set; get; }
        public bool? IsTrueHitAdverseMedia { set; get; }
        public int WatchlistStatusId { set; get; }
    }
}
