using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.Common.RBAScreening;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.RBADailyScoring.DTOs;
using static Tranglo1.Onboarding.Domain.DomainServices.RBAService;

namespace Tranglo1.RBADailyScoring.Jobs
{
    public class RBADailyScoringJob
    {
        private IConfiguration Configuration;

        public RBADailyScoringJob(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task Execute()
        {
            Log.Information("Execute RBADailyScoringJob");

            try
            {
                List<int> businessProfileCodes = new List<int>();

                businessProfileCodes = await GetBusinessProfileCodes();

                if (businessProfileCodes.Count > 0)
                {
                    Log.Information($"businessProfileCodes.Count: {businessProfileCodes.Count}");

                    foreach (var code in businessProfileCodes)
                    {
                        Log.Information($"BusinessProfileCode: {code}");

                        bool rbaStatus = await DoRBACalculation(code);

                        Log.Information($"DoRBACalculation() status: {rbaStatus}, code: {code}");
                    }
                }
                else
                {
                    Log.Information($"businessProfileCodes.Count: {businessProfileCodes.Count}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private async Task<bool> DoRBACalculation(int businessProfileCode)
        {
            Log.Information($"Execute DoRBACalculation({businessProfileCode})");

            bool status = false;

            try
            {
                var baseUrl = Configuration["RBAAPI"];

                CorporateRiskEvaluationRequestDTO requestDTO = new CorporateRiskEvaluationRequestDTO();
                List<RBA> rbaResultList = new List<RBA>();

                var screeningInput = await GetScreening(businessProfileCode);
                Log.Information($"screeningInput = await GetScreening({businessProfileCode}); > {JsonConvert.SerializeObject(screeningInput)}");

                var businessProfile = await GetBusinessProfile(businessProfileCode);
                Log.Information($"businessProfile = await GetBusinessProfile({businessProfileCode}); > {JsonConvert.SerializeObject(businessProfile)}");

                var partner = await GetPartnerRegistration(businessProfileCode);
                Log.Information($"partner = await GetPartnerRegistration({businessProfileCode}); > {JsonConvert.SerializeObject(partner)}");

                var businessNature = businessProfile.BusinessNatureCode.HasValue ? Enumeration.FindById<BusinessNature>((long)businessProfile.BusinessNatureCode) : null;
                var countryMeta = businessProfile.CompanyRegisteredCountryCode.HasValue ? Enumeration.FindById<CountryMeta>((long)businessProfile.CompanyRegisteredCountryCode) : null;
                var collectionTier = businessProfile.CollectionTierCode.HasValue ? Enumeration.FindById<CollectionTier>((long)businessProfile.CollectionTierCode) : null;
                var nationalityMeta = businessProfile.NationalityMetaCode.HasValue ? Enumeration.FindById<CountryMeta>((long)businessProfile.NationalityMetaCode) : null;

                string _collectionTier = null;
                if (businessProfile.CollectionTierCode.HasValue)
                {
                    _collectionTier = string.IsNullOrEmpty(collectionTier.Name) ? null : collectionTier.Name.Replace(" ", "");
                }

                // construct the complete API URL by appending the route components
                if (partner.CustomerTypeCode != CustomerType.Individual.Id)
                {
                    //create the request object based on screening data
                    requestDTO = new CorporateRiskEvaluationRequestDTO
                    {
                        IncorporationDate = businessProfile.DateOfIncorporation,
                        IncorporationCountry = countryMeta?.CountryISO2,
                        IncorporationType = businessProfile.IncorporationCompanyTypeCode,
                        IndustrySector = businessNature?.Id, //businessProfile.BusinessNatureCode,
                        CustomerCategory = partner.CustomerCategoryCode,
                        CollectionTier = _collectionTier,
                        IsPEP = screeningInput.IsPEP,
                        EnforcementActionTakenByRegulator = screeningInput.IsEnforcementAction,
                    };
                }

                var partnerSubscription = await GetPartnerSubscriptions(partner.PartnerCode);
                Log.Information($"partnerSubscription = await GetPartnerSubscriptions({partner.PartnerCode}); > {JsonConvert.SerializeObject(partnerSubscription)}");

                List<RBAPartnerSubscrption> groupPartnerSubs = new List<RBAPartnerSubscrption>();

                // currently only call RBA for TB + Entities
                groupPartnerSubs = partnerSubscription.Where(x => x.SolutionCode == Solution.Business.Id)
                    .GroupBy(x => new { x.SolutionCode, x.TrangloEntity })
                    .Select(grp => new RBAPartnerSubscrption
                    {
                        Solution = grp.Key.SolutionCode.HasValue ? Enumeration.FindById<Solution>((long)grp.Key.SolutionCode) : null,
                        TrangloEntity = grp.Key.TrangloEntity
                    }).ToList();
                Log.Information($"groupPartnerSubs = {JsonConvert.SerializeObject(groupPartnerSubs)}");

                // need to Call RBA for different entities
                foreach (var partnerSub in groupPartnerSubs)
                {
                    string apiUrl = string.Empty;

                    var apiServiceType = GetAPIServiceType(businessProfile.ServiceTypeCode);

                    // construct the complete API URL by appending the route components
                    if (partner.CustomerTypeCode != CustomerType.Individual.Id)
                    {
                        apiUrl = $"{baseUrl}/risk-evaluations/{partnerSub.TrangloEntity}/tranglo-business/corporate/{apiServiceType}";
                    }

                    string rbaJson = JsonConvert.SerializeObject(requestDTO);

                    // make the API request
                    var response = await MakeApiRequestAsync(apiUrl, rbaJson, apiServiceType, partnerSub.TrangloEntity);
                    Log.Information($"var response = await MakeApiRequestAsync({apiUrl},{rbaJson},{apiServiceType},{partnerSub.TrangloEntity}) > {JsonConvert.SerializeObject(response)}");

                    // Check the response and read the content if needed
                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information($"if (response.IsSuccessStatusCode) > {response.IsSuccessStatusCode}");
                        var result = await response.Content.ReadAsStringAsync();

                        var riskEvaluationResponse = JsonConvert.DeserializeObject<RiskEvaluationResponse>(result);

                        RBA processedRBAResult = RBAResultMapping(riskEvaluationResponse, businessProfile.BusinessProfileCode, partnerSub.Solution, partnerSub.TrangloEntity);

                        rbaResultList.Add(processedRBAResult);
                    }
                    else
                    {
                        // POST RBA failed
                        Log.Information($"else (response.IsSuccessStatusCode) > {response.IsSuccessStatusCode}");
                        Log.Error($"POST RBA Failed for code: {businessProfile.BusinessProfileCode}");
                    }
                }

                Log.Information($"rbaResultList.Count: {rbaResultList.Count}");

                // save RBA Results
                if (rbaResultList.Count > 0)
                {
                    var rbaStatus = await StoreRBAResponse(businessProfile.BusinessProfileCode, businessNature?.Id, collectionTier?.Name, nationalityMeta?.CountryISO2,
                        businessProfile.DateOfIncorporation, countryMeta?.CountryISO2, businessProfile.IncorporationCompanyTypeCode, partner.CustomerTypeCode, screeningInput.IsPEP, screeningInput.IsEnforcementAction, rbaResultList);
                    Log.Information($"var rbaStatus = await StoreRBAResponse({businessProfile.BusinessProfileCode}, {businessNature?.Id}, {collectionTier?.Name}, " +
                        $"{nationalityMeta?.CountryISO2}, {businessProfile.DateOfIncorporation}, {countryMeta?.CountryISO2}, {businessProfile.IncorporationCompanyTypeCode}, " +
                        $"{partner.CustomerTypeCode}, {screeningInput.IsPEP}, {screeningInput.IsEnforcementAction}, {JsonConvert.SerializeObject(rbaResultList)}) > {rbaStatus}");

                    status = rbaStatus;
                }

            }
            catch (System.NullReferenceException nullEx)
            {
                Log.Error(nullEx.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return status;
        }

        private async Task<ScreeningInputDTO> GetScreening(int businessProfileCode)
        {
            Log.Information($"Execute GetScreening({businessProfileCode})");

            ScreeningInputDTO screeningInput = new ScreeningInputDTO();

            try
            {
                List<ScreeningInputsDTO> screeningInputsDTOs = new List<ScreeningInputsDTO>();
                List<ScreeningResultsDTO> screeningResults = new List<ScreeningResultsDTO>();
                WatchlistReviewDTO watchlist = new WatchlistReviewDTO();

                //NOTE: based on GetScreeningInputsByBusinessProfileIdAsync()
                screeningInputsDTOs = await GetScreeningInput(businessProfileCode);
                Log.Information($"screeningInputsDTOs = await GetScreeningInput({businessProfileCode}); > {JsonConvert.SerializeObject(screeningInputsDTOs)}");

                // check all screening inputs is it all watchlist is Reviewed
                foreach (var item in screeningInputsDTOs)
                {
                    watchlist = await GetWatchlist(item.ScreeningInputCode);
                    Log.Information($"watchlist = await GetWatchlist({item.ScreeningInputCode}); > {JsonConvert.SerializeObject(watchlist)}");

                    if (item.OwnershipStrucureTypeId == OwnershipStrucureType.BusinessProfile.Id || item.OwnershipStrucureTypeId == OwnershipStrucureType.ParentHoldings.Id ||
                        item.OwnershipStrucureTypeId == OwnershipStrucureType.BoardOfDirector.Id || item.OwnershipStrucureTypeId == OwnershipStrucureType.PrimaryOfficer.Id ||
                        item.OwnershipStrucureTypeId == OwnershipStrucureType.AffiliatesAndSubsidiaries.Id || item.OwnershipStrucureTypeId == OwnershipStrucureType.AuthorisedPerson.Id)
                    {
                        screeningResults.Add(new ScreeningResultsDTO
                        {
                            OwnershipStrucureType = item.OwnershipStrucureTypeId,
                            IsPEP = watchlist == null ? (item.PEPCount > 0) : (watchlist.IsTrueHitPEP.HasValue ? (bool)watchlist.IsTrueHitPEP : false),
                            IsAdverseMedia = watchlist == null ? (item.AdverseMediaCount > 0) : (watchlist.EnforcementActionsCode == EnforcementActions.Yes.Id ? true : false),
                            IsWatchList = watchlist == null ? false : true,
                            WatchlistStatus = watchlist == null ? null : item.WatchlistStatus == null ? (int)WatchlistStatus.Reviewed.Id : item.WatchlistStatus
                        });
                    }
                    else if (item.OwnershipStrucureTypeId == OwnershipStrucureType.IndividualShareholder.Id || item.OwnershipStrucureTypeId == OwnershipStrucureType.IndividualShareholder.Id ||
                            item.OwnershipStrucureTypeId == OwnershipStrucureType.CompanyLegalEntity.Id || item.OwnershipStrucureTypeId == OwnershipStrucureType.IndividualLegalEntity.Id)
                    {
                        var ownership = await GetOwnershipDetails(item.OwnershipStrucureTypeId, item.TableId);
                        Log.Information($"_effectiveShareholding = await GetOwnershipDetails({item.OwnershipStrucureTypeId}, {item.TableId}); > {ownership.EffectiveShareholding}");

                        screeningResults.Add(new ScreeningResultsDTO
                        {
                            OwnershipStrucureType = OwnershipStrucureType.CompanyLegalEntity.Id,
                            IsPEP = watchlist == null ? (item.PEPCount > 0) : (watchlist.IsTrueHitPEP.HasValue ? (bool)watchlist.IsTrueHitPEP : false),
                            IsAdverseMedia = watchlist == null ? (item.AdverseMediaCount > 0) : (watchlist.EnforcementActionsCode == EnforcementActions.Yes.Id ? true : false),
                            IsWatchList = watchlist == null ? false : true,
                            WatchlistStatus = watchlist == null ? null : item.WatchlistStatus == null ? (int)WatchlistStatus.Reviewed.Id : item.WatchlistStatus,
                            PercentageOfOwnership = Decimal.TryParse(ownership.EffectiveShareholding, out decimal result) ? result : 0
                        });
                    }
                }                

                if (!screeningResults.Any(x => x.IsWatchList && x.WatchlistStatus != WatchlistStatus.Reviewed.Id))
                {
                    bool isPEP = false;
                    bool isEnforcementAction = false;

                    double value = 1.0 / 3.0;

                    // checking for IsPEP ShareHolders or Legal Entities >= 25%, Any PrimaryOfficer or AuthorisedPerson IsPEP
                    if ((screeningResults.Where(x => (x.OwnershipStrucureType == OwnershipStrucureType.CompanyShareholder.Id || x.OwnershipStrucureType == OwnershipStrucureType.IndividualShareholder.Id) && x.IsPEP).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.OwnershipStrucureType == OwnershipStrucureType.CompanyLegalEntity.Id || x.OwnershipStrucureType == OwnershipStrucureType.IndividualLegalEntity.Id) && x.IsPEP).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.OwnershipStrucureType == OwnershipStrucureType.PrimaryOfficer.Id || x.OwnershipStrucureType == OwnershipStrucureType.AuthorisedPerson.Id)).Any(y => y.IsPEP)))
                    {
                        isPEP = true;
                    }

                    // checking for Enforcement Action (IsAdverseMedia) ShareHolders or Legal Entities >= 25%, Any PrimaryOfficer or AuthorisedPerson Enforcement Action
                    if ((screeningResults.Where(x => (x.OwnershipStrucureType == OwnershipStrucureType.CompanyShareholder.Id || x.OwnershipStrucureType == OwnershipStrucureType.IndividualShareholder.Id) && (x.IsAdverseMedia)).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.OwnershipStrucureType == OwnershipStrucureType.CompanyLegalEntity.Id || x.OwnershipStrucureType == OwnershipStrucureType.IndividualLegalEntity.Id) && (x.IsAdverseMedia)).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.OwnershipStrucureType == OwnershipStrucureType.PrimaryOfficer.Id || x.OwnershipStrucureType == OwnershipStrucureType.AuthorisedPerson.Id)).Any(y => y.IsAdverseMedia)))
                    {
                        isEnforcementAction = true;
                    }

                    if (screeningResults.Where(x => x.OwnershipStrucureType == OwnershipStrucureType.BoardOfDirector.Id).Count() > 0)
                    {
                        double bodPEP = screeningResults.Where(x => x.OwnershipStrucureType == OwnershipStrucureType.BoardOfDirector.Id && x.IsPEP).Count() / screeningResults.Where(x => x.OwnershipStrucureType == OwnershipStrucureType.BoardOfDirector.Id).Count();
                        double bodEnforcementAction = screeningResults.Where(x => x.OwnershipStrucureType == OwnershipStrucureType.BoardOfDirector.Id && x.IsAdverseMedia).Count() / screeningResults.Where(x => x.OwnershipStrucureType == OwnershipStrucureType.BoardOfDirector.Id).Count();

                        if (bodPEP >= value)
                        {
                            isPEP = true;
                        }

                        if (bodEnforcementAction >= value)
                        {
                            isEnforcementAction = true;
                        }
                    }

                    screeningInput.IsPEP = isPEP;
                    screeningInput.IsEnforcementAction = isEnforcementAction;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return screeningInput;
        }

        private async Task<bool> StoreRBAResponse(int businessProfileCode, long? businessNatureId, string collectionTier, string nationality,
            DateTime? dateOfIncorporation, string countryOfIncorporation, long? incorporationCompanyTypeCode, long customerTypeCode,
            bool IsPEP, bool IsEnforcementAction, List<RBA> rbaResultList)
        {
            //NOTE: based on SaveRBAScreeningInputAsync()
            Log.Information("Execute StoreRBAResponse");

            bool storeStatus = false;

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "SaveRBADailyScoringJSON",
                    new
                    {
                        BusinessProfileCode = businessProfileCode,
                        IndustrySector = businessNatureId,
                        CollectionTier = collectionTier,
                        Nationality = nationality,
                        IncorporationDate = dateOfIncorporation,
                        IncorporationCountry = countryOfIncorporation,
                        IncorporationType = incorporationCompanyTypeCode,
                        CustomerCategory = customerTypeCode,
                        IsPEP = IsPEP,
                        EnforcementActionTakenByRegulator = IsEnforcementAction,
                        JSON = JsonConvert.SerializeObject(rbaResultList)
                    },
                    null, null, CommandType.StoredProcedure);

                IEnumerable<int> resReader = await reader.ReadAsync<int>();

                storeStatus = Convert.ToBoolean(resReader.FirstOrDefault());
            }

            return storeStatus;
        }

        #region RBADailyScoring
        private async Task<List<int>> GetBusinessProfileCodes()
        {
            Log.Information("Execute GetBusinessProfileCodes");

            List<int> businessProfileCodes = new List<int>();

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetBusinessProfileCodesForDailyScoring",
                    new { },
                    null, null, CommandType.StoredProcedure);

                List<BusinessProfileDTO> businessProfileDTO = (List<BusinessProfileDTO>)await reader.ReadAsync<BusinessProfileDTO>();

                if (businessProfileDTO.Count > 0)
                {
                    foreach (var profile in businessProfileDTO)
                    {
                        businessProfileCodes.Add(profile.BusinessProfileCode);
                    }
                }
            }

            return businessProfileCodes;
        }

        private async Task<BusinessProfileDTO> GetBusinessProfile(long businessProfileCode)
        {
            //NOTE: based on GetBusinessProfileByCodeAsync()
            Log.Information("Execute GetBusinessProfile");

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            BusinessProfileDTO businessProfileDTOs = new BusinessProfileDTO();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetBusinessProfileForDailyScoring",
                    new
                    {
                        businessProfileCode = businessProfileCode
                    },
                    null, null, CommandType.StoredProcedure);

                businessProfileDTOs = reader.ReadFirstOrDefault<BusinessProfileDTO>();
            }

            return businessProfileDTOs;
        }

        private async Task<PartnerRegistrationDTO> GetPartnerRegistration(long businessProfileCode)
        {
            //NOTE: based on GetPartnerRegistrationByBusinessProfileCodeAsync()
            Log.Information("Execute GetPartnerRegistration");

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            PartnerRegistrationDTO partnerRegistrationDTO = new PartnerRegistrationDTO();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetPartnerRegistrationForDailyScoring",
                    new
                    {
                        businessProfileCode = businessProfileCode
                    },
                    null, null, CommandType.StoredProcedure);

                partnerRegistrationDTO = reader.ReadFirstOrDefault<PartnerRegistrationDTO>();
            }

            return partnerRegistrationDTO;
        }

        private async Task<List<PartnerSubscriptionDTO>> GetPartnerSubscriptions(long partnerCode)
        {
            //NOTE: based on GetSalesPartnerSubscriptionListAsync()
            Log.Information("Execute GetPartnerSubscriptions");

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            List<PartnerSubscriptionDTO> partnerSubscriptionDTOs = new List<PartnerSubscriptionDTO>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetPartnerSubscriptionsForDailyScoring",
                    new
                    {
                        partnerCode = partnerCode
                    },
                    null, null, CommandType.StoredProcedure);

                partnerSubscriptionDTOs = (List<PartnerSubscriptionDTO>)await reader.ReadAsync<PartnerSubscriptionDTO>();
            }

            return partnerSubscriptionDTOs;
        }

        private async Task<List<ScreeningInputsDTO>> GetScreeningInput(int businessProfileCode)
        {
            Log.Information("Execute GetScreeningInput");

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            List<ScreeningInputsDTO> screeningInputsDTOs = new List<ScreeningInputsDTO>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetScreeningInputForDailyScoring",
                    new
                    {
                        businessProfileCode = businessProfileCode
                    },
                    null, null, CommandType.StoredProcedure);

                screeningInputsDTOs = (List<ScreeningInputsDTO>)await reader.ReadAsync<ScreeningInputsDTO>();
            }

            return screeningInputsDTOs;
        }

        private async Task<WatchlistReviewDTO> GetWatchlist(int screeningInputCode)
        {
            //NOTE: based on GetWatchlistReviewByScreeningInputCode
            Log.Information("Execute GetWatchlist");

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            WatchlistReviewDTO watchlistReviewDTO = new WatchlistReviewDTO();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetWatchlistForDailyScoring",
                    new
                    {
                        screeningInputCode = screeningInputCode
                    },
                    null, null, CommandType.StoredProcedure);

                watchlistReviewDTO = await reader.ReadFirstOrDefaultAsync<WatchlistReviewDTO>();
            }

            return watchlistReviewDTO;
        }

        private async Task<OwnershipDetailDTO> GetOwnershipDetails(int ownershipStrucureTypeId, long tableId)
        {
            Log.Information("Execute GetOwnershipDetails");

            var _connectionString = Configuration.GetConnectionString("CustomerIdentityServer");

            OwnershipDetailDTO ownershipDetailDTO = new OwnershipDetailDTO();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var reader = await connection.QueryMultipleAsync(
                    "GetOwnershipDetailsForDailyScoring",
                    new
                    {
                        ownershipStrucureTypeId = ownershipStrucureTypeId,
                        tableId = tableId
                    },
                    null, null, CommandType.StoredProcedure);

                ownershipDetailDTO = await reader.ReadFirstOrDefaultAsync<OwnershipDetailDTO>();
            }

            return ownershipDetailDTO;
        }
        #endregion

        #region RBAService
        private async Task<HttpResponseMessage> MakeApiRequestAsync(string apiUrl, string rbaJson, string apiServiceType, string trangloEntity)
        {
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(rbaJson, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);

                return response;
            }
        }

        private RBA RBAResultMapping(RiskEvaluationResponse rbaResponse, int businessProfileId, Solution solution, string trangloEntity)
        {
            List<EvaluationRules> evaluationRules = new List<EvaluationRules>();

            foreach (var evaluationRule in rbaResponse.EvaluationRules)
            {
                EvaluationRules rule = new EvaluationRules
                {
                    Template = evaluationRule.Template,
                    Score = evaluationRule.Score,
                    ActualValue = evaluationRule.ActualValue,
                    CriticalRanking = evaluationRule.CriticalRanking,
                    IsMatched = evaluationRule.IsMatched,
                    Parameter = new EvaluationRulesParameter
                    {
                        Name = evaluationRule.Parameter.Name,
                        Description = evaluationRule.Parameter.Description
                    }
                };

                evaluationRules.Add(rule);
            }

            List<OverridingRules> overridingRules = new List<OverridingRules>();

            foreach (var overridingRule in rbaResponse.OverridingRules)
            {
                OverridingRules rule = new OverridingRules
                {
                    Template = overridingRule.Template,
                    ActualValue = overridingRule.ActualValue,
                    IsMatched = overridingRule.IsMatched,
                    Parameter = new OverridingRulesParameter
                    {
                        Name = overridingRule.Parameter.Name,
                        Description = overridingRule.Parameter.Description
                    }
                };

                overridingRules.Add(rule);
            }

            var rba = new RBA
            {
                BusinessProfileCode = businessProfileId,
                ResultId = rbaResponse.ResultId,
                RiskScore = rbaResponse.RiskScore,
                RiskRanking = rbaResponse.RiskRanking,
                Solution = solution,
                RBAPlatformDescription = rbaResponse.Platform,
                ScreeningEntityType = MapEntityTypeToEnum(rbaResponse.EntityType),
                RBAEntityType = rbaResponse.EntityType,
                PartnerType = MapPartnerTypeToEnum(rbaResponse.PartnerType),
                RBAPartnerType = rbaResponse.PartnerType,
                EvaluationRules = evaluationRules,
                OverridingRules = overridingRules,
                TrangloEntity = trangloEntity,
                RBAScreeningDate = DateTime.UtcNow
                // Map other properties...
            };

            return rba;
        }

        private string GetAPIServiceType(long? serviceTypeCode)
        {
            switch (serviceTypeCode)
            {
                case 1:
                    return "CollectionAndPayout";
                case 2:
                    return "CollectionAndPayout";
                case 3:
                    return "Payout";
                default:
                    return "";
            }
        }

        #endregion
    }
}
