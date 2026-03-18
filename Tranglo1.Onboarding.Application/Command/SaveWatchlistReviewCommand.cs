using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Watchlist;


namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.Compliance, UACAction.Edit)]
    internal class SaveWatchlistReviewCommand : BaseCommand<Result<WatchlistReviewOutputDTO>>
    {
        public long ScreeningInputCode { get; set; }
        public WatchlistReviewInputDTO InputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<WatchlistReviewOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string description = result.Value.IsKIV ? "Edited Personnel Watchlist (KIV)" : "Edited Personnel Watchlist";
                return Task.FromResult(description);
            }

            return Task.FromResult<string>(null);
        }

        public class SaveWatchlistReviewCommandHandler : IRequestHandler<SaveWatchlistReviewCommand, Result<WatchlistReviewOutputDTO>>
        {
            private readonly IScreeningRepository _screeningRepository;
            private readonly RBAService _rbaService;

            public SaveWatchlistReviewCommandHandler(
              IScreeningRepository screeningRepository,
              RBAService rbaService
          )
            {
                _screeningRepository = screeningRepository;
                _rbaService = rbaService;
            }

            public async Task<Result<WatchlistReviewOutputDTO>> Handle(SaveWatchlistReviewCommand request, CancellationToken cancellationToken)
            {
                var screeningInput = await _screeningRepository.GetScreeningInputById(request.ScreeningInputCode);

                if (screeningInput == null)
                {
                    return Result.Failure<WatchlistReviewOutputDTO>($"Screening Input is not valid: {request.ScreeningInputCode}.");
                }

                var watchlistReview = new WatchlistReview(
                    screeningInput: screeningInput,
                    isTrueHitPEP: request.InputDTO.IsPEP,
                    isTrueHitSanction: request.InputDTO.IsSanction,
                    isTrueHitSOE: request.InputDTO.IsSOE,
                    isTrueHitAdverseMedia: request.InputDTO.IsAdverseMedia,
                    isTrueHitEnforcement: request.InputDTO.IsEnforcement,
                    watchlistStatus: request.InputDTO.GetWatchlistStatus(),
                    remarks: request.InputDTO.Remarks,
                    enforcementActions: request.InputDTO.GetEnforcementAction()
                    );

                var result = await _screeningRepository.AddWatchlistReview(watchlistReview, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<WatchlistReviewOutputDTO>($"There is an issue during add watchlist review: {request.ScreeningInputCode}.");
                }

                var watchlistReviewOutputDTO = new WatchlistReviewOutputDTO();
                if (result.IsSuccess)
                {
                    watchlistReviewOutputDTO.WatchlistReviewCode = result.Value.Id;
                    watchlistReviewOutputDTO.IsKIV = request.InputDTO.IsKIV;
                }

                await _rbaService.ProcessRiskEvaluationsWithWatchListAsync(screeningInput);

                return Result.Success<WatchlistReviewOutputDTO>(watchlistReviewOutputDTO);
            }
        }
    }
}