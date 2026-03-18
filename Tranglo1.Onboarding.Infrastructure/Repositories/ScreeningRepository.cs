using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using System.Linq;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using System.Threading;

namespace Tranglo1.Onboarding.Infrastructure.Repositories
{
    public class ScreeningRepository : IScreeningRepository
    {
        private readonly ScreeningDBContext dbContext;

        public ScreeningRepository(ScreeningDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Screening>> GetScreening()
        {
            var query = dbContext.Screenings;
            return await query.ToListAsync();
        }
        public async Task<ScreeningInput> GetScreeningInputById(long screeningInputCode)
        {
            var query = await this.dbContext.ScreeningInputs.FirstOrDefaultAsync(x => x.Id == screeningInputCode);
            return query;
        }
        public async Task<WatchlistReview> GetWatchlistReviewById(long watchlistReviewCode)
        {
            var query = await this.dbContext.WatchlistReviews.FirstOrDefaultAsync(x => x.Id == watchlistReviewCode);
            return query;
        }

        public async Task<Result<WatchlistReview>> AddWatchlistReview(WatchlistReview watchlistReview, CancellationToken cancellationToken)
        {
            this.dbContext.WatchlistReviews.Add(watchlistReview);
            await this.dbContext.SaveChangesAsync(cancellationToken);
            return watchlistReview;
        }

        public async Task<Result<WatchlistReviewDocument>> AddWatchlistReviewDocument(WatchlistReviewDocument watchlistReviewDocument, CancellationToken cancellationToken)
        {
            this.dbContext.WatchlistReviewDocuments.Add(watchlistReviewDocument);
            await this.dbContext.SaveChangesAsync(cancellationToken);
            return watchlistReviewDocument;
        }

        public async Task<EnforcementActions> GetEnforcementActionsByCode(long enforcementActionsCode)
        {
            var query = dbContext.EnforcementActions
                .Where(x => x.Id == enforcementActionsCode)
                .FirstOrDefaultAsync();
            return await query;
        }

        public async Task<Screening> GetScreeningByScreeningCode(string clientReference)
        {
            if (int.TryParse(clientReference, out int screeningCode))
            {
                var query = await this.dbContext.Screenings.FirstOrDefaultAsync(x => x.Id == screeningCode);
                return query;
            }
            else
            {

                return null;
            }
        }

        public async Task<WatchlistReview> GetWatchlistReviewByScreeningInputCode(long screeningInputCode)
        {
            var query = await this.dbContext.WatchlistReviews.Where(x => x.ScreeningInput.Id == screeningInputCode).OrderBy(y => y.Id).LastOrDefaultAsync();
            return query;
        }
        public async Task<ScreeningInput> GetScreeningInputAndBusinessProfileById(long screeningInputCode)
        {
            var query = await this.dbContext.ScreeningInputs.Include(x => x.BusinessProfile).ThenInclude(x => x.ServiceType).FirstOrDefaultAsync(x => x.Id == screeningInputCode);
            return query;
        }

        public async Task DeleteScreeningInputsByMyPropertyIds(IEnumerable<long> myPropertyCodes, long businessProfileCode)
        {
            foreach (var myPropertyCode in myPropertyCodes)
            {
                var screeningInputsToDeleteIds = await this.dbContext.ScreeningInputs
                    .Where(si => si.TableId == myPropertyCode && si.BusinessProfile.Id == businessProfileCode)
                    .Select(si => si.Id)
                    .ToListAsync();

                foreach (var screeningInputToDeleteId in screeningInputsToDeleteIds)
                {
                    var screeningIdsToDelete = await this.dbContext.Screenings
                        .Where(s => s.ScreeningInput.Id == screeningInputToDeleteId)
                        .Select(s => s.Id)
                        .ToListAsync();

                    foreach (var screeningId in screeningIdsToDelete)
                    {
                        var screeningToDelete = await this.dbContext.Screenings.FindAsync((long)screeningId);

                        if (screeningToDelete != null)
                        {
                            var screeningDetailsToDelete = await this.dbContext.ScreeningDetails
                                .Where(sd => sd.Screening.Id == screeningToDelete.Id)
                                .ToListAsync();

                            this.dbContext.ScreeningDetails.RemoveRange(screeningDetailsToDelete);
                            await this.dbContext.SaveChangesAsync();

                            this.dbContext.Screenings.Remove(screeningToDelete);
                            await this.dbContext.SaveChangesAsync();
                        }
                    }

                    var screeningInputToDelete = await this.dbContext.ScreeningInputs.FindAsync(screeningInputToDeleteId);
                    if (screeningInputToDelete != null)
                    {
                        this.dbContext.ScreeningInputs.Remove(screeningInputToDelete);
                        await this.dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
