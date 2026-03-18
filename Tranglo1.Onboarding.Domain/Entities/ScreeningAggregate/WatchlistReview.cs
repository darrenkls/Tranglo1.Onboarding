using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class WatchlistReview : Entity
    {
        public ScreeningInput ScreeningInput { set; get; }
        public bool? IsTrueHitPEP { set; get; }
        public bool? IsTrueHitSanction { set; get; }
        public bool? IsTrueHitSOE { set; get; }
        public bool? IsTrueHitAdverseMedia { set; get; }
        public bool? IsTrueHitEnforcement { set; get; }
        public WatchlistStatus WatchlistStatus { set; get; }
        public string Remarks { set; get; }
        public EnforcementActions EnforcementActions { set; get; }

        public WatchlistReview()
        {

        }

        public WatchlistReview(ScreeningInput screeningInput, 
            bool? isTrueHitPEP,
            bool? isTrueHitSanction,
            bool? isTrueHitSOE, 
            bool? isTrueHitAdverseMedia,
            bool? isTrueHitEnforcement,
            WatchlistStatus watchlistStatus,
            string remarks,
            EnforcementActions enforcementActions)
        {
            ScreeningInput = screeningInput;

            IsTrueHitPEP = isTrueHitPEP;
            IsTrueHitSanction = isTrueHitSanction;
            IsTrueHitSOE = isTrueHitSOE;
            IsTrueHitAdverseMedia = isTrueHitAdverseMedia;
            IsTrueHitEnforcement = isTrueHitEnforcement;
            WatchlistStatus = watchlistStatus;
            Remarks = remarks;
            EnforcementActions = enforcementActions;

            screeningInput.ReviewScreeningResult(watchlistStatus);
        }
    }
}
