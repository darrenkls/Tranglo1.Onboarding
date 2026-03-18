using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common.SingleScreening;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Watchlist
{
    public interface IWatchlistNotificationExternalService
    {
        Task<Result<bool, string>> SendAsync(List<ChangeDTO> changeDTOs, bool isSingleProfileScreening, string singlePartnerName);
    }
}
