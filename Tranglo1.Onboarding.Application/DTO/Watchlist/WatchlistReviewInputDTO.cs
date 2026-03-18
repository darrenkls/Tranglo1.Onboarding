using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Application.DTO.Watchlist
{
    public class WatchlistReviewInputDTO
    {
        public bool IsSanction { get; set; } = false;
        public bool IsPEP { get; set; } = false;
        public bool IsSOE { get; set; } = false;
        public bool IsAdverseMedia { get; set; } = false;
        public bool IsEnforcement { get; set; } = false;
        public string Remarks { get; set; }
        public bool IsKIV { get; set; }
        public bool IsEnforcementActionTaken { get; set; } = false;

        /// <summary>
        /// Helpers to convert IsEnforcementActionTaken boolean to Enumeration
        /// </summary>
        /// <returns></returns>
        public EnforcementActions GetEnforcementAction()
        {
            return IsEnforcementActionTaken ?
                    EnforcementActions.Yes :
                    EnforcementActions.No;
        }

        /// <summary>
        /// Helpers to convert IsKIV boolean to WatchlistStatus Enumeration
        /// </summary>
        /// <returns></returns>
        public WatchlistStatus GetWatchlistStatus()
        {
            return IsKIV ?
                WatchlistStatus.KIV : 
                WatchlistStatus.Reviewed;
        }
    }
}
