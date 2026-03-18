using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class WatchlistDocumentsDownloadDTO
    {
        public int WatchlistCode { get; set; }
        public Guid DocumentId { get; set; }
    }
}
