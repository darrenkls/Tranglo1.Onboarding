using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class WatchlistStatus : Enumeration
    {
        public WatchlistStatus(int id, string name) : base(id, name)
        {
        }

        public WatchlistStatus()
        {
        }

        public static readonly WatchlistStatus PendingReview = new WatchlistStatus(1, "Pending Review");
        public static readonly WatchlistStatus KIV = new WatchlistStatus(2, "KIV");
        public static readonly WatchlistStatus Reviewed = new WatchlistStatus(3, "Reviewed");

    }
}
