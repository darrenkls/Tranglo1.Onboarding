using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Requests;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses;

namespace Tranglo1.Onboarding.Domain.ExternalServices.Compliance
{
    public interface IComplianceExternalService
    {
        Task<Result<NameScreenerResponse, string>> ScreeningAsync(NameScreenerRequest request);
        Task<Result<IReadOnlyList<BulkNameScreenerResponse>, string>> ScreeningAsync(IEnumerable<BulkNameScreenerRequest> request);
        Task<Result<IReadOnlyList<GetEntityDetailByReferenceCodeResponse>, string>> GetEntityDetailsByReferenceCodeAsync(Guid reference);
        Task<Result<GetEntityDetailByReferenceCodeAndEntityIdResponse, string>> GetEntityDetailAsync(Guid referenceCode, long entityId, ScreeningListSource screeningListSource);
    }
}
