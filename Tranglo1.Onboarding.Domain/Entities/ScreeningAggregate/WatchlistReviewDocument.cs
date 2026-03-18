using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class WatchlistReviewDocument: Entity
    {
        public WatchlistReview WatchlistReview { set; get; }
        public Guid DocumentId { get; set; }

        public WatchlistReviewDocument()
        {
        }
        public WatchlistReviewDocument(WatchlistReview watchlistReview, Guid documentId)
        {
            this.WatchlistReview = watchlistReview;
            this.DocumentId = documentId;
        }
    }
}
