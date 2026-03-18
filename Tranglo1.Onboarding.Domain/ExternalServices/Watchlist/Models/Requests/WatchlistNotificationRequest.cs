using System.Collections.Generic;
using Tranglo1.Onboarding.Domain.Common.SingleScreening;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Watchlist.Models.Requests
{
    public class WatchlistNotificationRequest
    {
        public List<ChangeDTO> ChangeDTOs { get; set; }
        public bool IsSingleProfileScreening { get; set; }
        /// <summary>
        /// This property is only used for Single Profile Screening (after a KYC profile submitted for review)
        /// </summary>
        public string SinglePartnerName { get; set; }
    }
}
