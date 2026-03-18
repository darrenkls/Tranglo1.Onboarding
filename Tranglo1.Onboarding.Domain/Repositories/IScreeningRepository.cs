using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;

namespace Tranglo1.Onboarding.Domain.Repositories
{
    public interface IScreeningRepository
    {
        Task<IReadOnlyList<Screening>> GetScreening();
        Task<ScreeningInput> GetScreeningInputById(long screeningInputCode);
        Task<WatchlistReview> GetWatchlistReviewById(long watchlistReviewCode);
        Task<Result<WatchlistReview>> AddWatchlistReview(WatchlistReview watchlistReview, CancellationToken cancellationToken);
        Task<Result<WatchlistReviewDocument>> AddWatchlistReviewDocument(WatchlistReviewDocument watchlistReviewDocument, CancellationToken cancellationToken);
        Task<EnforcementActions> GetEnforcementActionsByCode(long enforcementActionsCode);
        Task<Screening> GetScreeningByScreeningCode(string clientReference);
        Task<WatchlistReview> GetWatchlistReviewByScreeningInputCode(long screeningInputCode);
        Task<ScreeningInput> GetScreeningInputAndBusinessProfileById(long screeningInputCode);
        Task DeleteScreeningInputsByMyPropertyIds(IEnumerable<long> myPropertyCodes, long businessProfileCode);
    }
}
