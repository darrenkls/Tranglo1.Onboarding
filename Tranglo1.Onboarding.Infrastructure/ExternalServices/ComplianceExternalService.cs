using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Requests;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses;
using Tranglo1.Onboarding.Infrastructure.Extensions;

namespace Tranglo1.Onboarding.Infrastructure.ExternalServices
{
    public class ComplianceExternalService : IComplianceExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ComplianceExternalService> _logger;

        public ComplianceExternalService(HttpClient httpClient, ILogger<ComplianceExternalService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<NameScreenerResponse, string>> ScreeningAsync(NameScreenerRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("name-screener", request);
                var result = await response.ReadFromJsonAsync<NameScreenerResponse>();

                return Result.Success<NameScreenerResponse, string>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Result.Failure<NameScreenerResponse, string>(ex.ToString());
            }
        }

        public async Task<Result<IReadOnlyList<BulkNameScreenerResponse>, string>> ScreeningAsync(IEnumerable<BulkNameScreenerRequest> request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("bulk-name-screener", request);
                var result = await response.ReadFromJsonAsync<IReadOnlyList<BulkNameScreenerResponse>>();

                return Result.Success<IReadOnlyList<BulkNameScreenerResponse>, string>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Result.Failure<IReadOnlyList<BulkNameScreenerResponse>, string>(ex.ToString());
            }
        }

        public async Task<Result<IReadOnlyList<GetEntityDetailByReferenceCodeResponse>, string>> GetEntityDetailsByReferenceCodeAsync(Guid reference)
        {
            try
            {
                var response = await _httpClient.GetAsync($"name-screener/{reference}/entities");
                var result = await response.ReadFromJsonAsync<IReadOnlyList<GetEntityDetailByReferenceCodeResponse>>();

                return Result.Success<IReadOnlyList<GetEntityDetailByReferenceCodeResponse>, string>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Result.Failure<IReadOnlyList<GetEntityDetailByReferenceCodeResponse>, string>(ex.ToString());
            }
        }

        public async Task<Result<GetEntityDetailByReferenceCodeAndEntityIdResponse, string>> GetEntityDetailAsync(Guid referenceCode, long entityId, ScreeningListSource screeningListSource)
        {
            try
            {
                var screeningListSourceApiPathName = screeningListSource?.GetApiPathName();
                if (string.IsNullOrWhiteSpace(screeningListSourceApiPathName))
                {
                    return Result.Failure<GetEntityDetailByReferenceCodeAndEntityIdResponse, string>("Invalid screening list source");
                }

                var response = await _httpClient.GetAsync($"name-screener/{referenceCode}/entities/{screeningListSourceApiPathName}/{entityId}");
                var result = await response.ReadFromJsonAsync<GetEntityDetailByReferenceCodeAndEntityIdResponse>();

                return Result.Success<GetEntityDetailByReferenceCodeAndEntityIdResponse, string>(result);
            }
            catch (Exception ex)
            {
                // Must use ex.Message instead of ex.ToString() to avoid AccessViolationException as below:
                // System.AccessViolationException: 'Attempted to read or write protected memory. This is often an indication that other memory is corrupt.'
                _logger.LogError(ex.ToString());
                return Result.Failure<GetEntityDetailByReferenceCodeAndEntityIdResponse, string>(ex.ToString());
            }
        }
    }
}
