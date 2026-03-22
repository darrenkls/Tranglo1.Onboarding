using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common.RBAScreening;
using Tranglo1.Onboarding.Domain.Common.SingleScreening;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.RBAAggregate;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Domain.Repositories;

namespace Tranglo1.Onboarding.Domain.DomainServices
{
    public class RBAService
    {
        private readonly IRBARepository rbaRepository;
        private readonly IScreeningRepository screeningRepository;
        private readonly IBusinessProfileRepository businessProfileRepository;
        private readonly IPartnerRepository partnerRepository;
        private readonly IConfiguration config;
        private readonly ILogger<RBAService> logger;

        public RBAService(
            IRBARepository rbaRepository,
            IScreeningRepository screeningRepository,
            IBusinessProfileRepository businessProfileRepository,
            IPartnerRepository partnerRepository,
            IConfiguration config,
            ILogger<RBAService> logger
            )
        {
            this.rbaRepository = rbaRepository;
            this.screeningRepository = screeningRepository;
            this.businessProfileRepository = businessProfileRepository;
            this.partnerRepository = partnerRepository;
            this.config = config;
            this.logger = logger;
        }

        public class RBAPartnerSubscrption
        {
            public Solution Solution { get; set; }
            public string TrangloEntity { get; set; }
        }

        private class ScreeningResult
        {
            public OwnershipStrucureType ownershipStrucureType { get; set; }
            public bool isPEP { get; set; }
            public bool isAdverseMedia { get; set; }
            public decimal PercentageOfOwnership { get; set; }
        }

        private class WatchListReview : ScreeningResult
        {
            public bool isWatchList { get; set; }
            public WatchlistStatus watchlistStatus { get; set; }
        }

        public async Task ProcessRiskEvaluationsAsync(List<SingleScreeningListResultOutputDTO> screeningDetails, int businessProfileId)
        {
            try
            {
                if (!screeningDetails.Any(x => (x.Summary.PEP > 0 || x.Summary.SanctionList > 0 || x.Summary.SOE > 0 || x.Summary.AdverseMedia > 0)))
                {
                    var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileId);

                    var partner = await partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                    var customerCategory = await partnerRepository.GetCustomerTypeByCodeAsync(partner.CustomerType.Id);

                    var partnerSubscription = await partnerRepository.GetSalesPartnerSubscriptionListAsync(partner.Id);

                    List<RBAPartnerSubscrption> groupPartnerSubs = new List<RBAPartnerSubscrption>();


                    // Currently only call RBA for TB + Entities
                    groupPartnerSubs = partnerSubscription.Where(x => x.Solution == Solution.Business)
                        .GroupBy(x => new { x.Solution, x.TrangloEntity })
                        .Select(grp => new RBAPartnerSubscrption
                        {
                            Solution = grp.Key.Solution,
                            TrangloEntity = grp.Key.TrangloEntity
                        }).ToList();


                    // PEPCount and EnformentAction both pass false value if no hit any from LN Screening
                    await ProcessRequestData(groupPartnerSubs, businessProfile, customerCategory, false, false);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing risk evaluations for Business Profile code {businessProfileId}.");
            }
        }


        public async Task ProcessRiskEvaluationsWithWatchListAsync(ScreeningInput screeningInput)
        {
            try
            {
                var individualScreeningInputData = await screeningRepository.GetScreeningInputAndBusinessProfileById(screeningInput.Id);

                var allScreeningInputs = await businessProfileRepository.GetScreeningInputsByBusinessProfileIdAsync(individualScreeningInputData.BusinessProfile.Id);

                var businessProfile = await businessProfileRepository.GetBusinessProfileByCodeAsync(individualScreeningInputData.BusinessProfile?.Id);
                var partner = await partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                var customerCategory = await partnerRepository.GetCustomerTypeByCodeAsync(partner.CustomerType.Id);

                List<WatchListReview> screeningResults = new List<WatchListReview>();

                // Check all screening inputs is it all watchlist is Reviewed
                foreach (var item in allScreeningInputs)
                {
                    var watchlist = await screeningRepository.GetWatchlistReviewByScreeningInputCode(item.Id);

                    if (item.OwnershipStrucureType == OwnershipStrucureType.BusinessProfile || item.OwnershipStrucureType == OwnershipStrucureType.ParentHoldings ||
                        item.OwnershipStrucureType == OwnershipStrucureType.BoardOfDirector || item.OwnershipStrucureType == OwnershipStrucureType.PrimaryOfficer ||
                        item.OwnershipStrucureType == OwnershipStrucureType.AffiliatesAndSubsidiaries || item.OwnershipStrucureType == OwnershipStrucureType.AuthorisedPerson)
                    {
                        screeningResults.Add(new WatchListReview
                        {
                            ownershipStrucureType = item.OwnershipStrucureType,
                            isPEP = watchlist == null ? (item.PEPCount > 0) : (watchlist.IsTrueHitPEP.HasValue ? (bool)watchlist.IsTrueHitPEP : false),
                            isAdverseMedia = watchlist == null ? (item.AdverseMediaCount > 0) : (watchlist.EnforcementActions == EnforcementActions.Yes ? true : false),
                            isWatchList = watchlist == null ? false : true,
                            watchlistStatus = watchlist == null ? null : item.WatchlistStatus == null ? WatchlistStatus.Reviewed : item.WatchlistStatus
                        });
                    }
                    else if (item.OwnershipStrucureType == OwnershipStrucureType.CompanyShareholder)
                    {
                        var companyShareHolder = await businessProfileRepository.GetCompanyShareholderByTableIDAsync(item.TableId);

                        screeningResults.Add(new WatchListReview
                        {
                            ownershipStrucureType = OwnershipStrucureType.CompanyShareholder,
                            isPEP = watchlist == null ? (item.PEPCount > 0) : (watchlist.IsTrueHitPEP.HasValue ? (bool)watchlist.IsTrueHitPEP : false),
                            isAdverseMedia = watchlist == null ? (item.AdverseMediaCount > 0) : (watchlist.EnforcementActions == EnforcementActions.Yes ? true : false),
                            isWatchList = watchlist == null ? false : true,
                            watchlistStatus = watchlist == null ? null : item.WatchlistStatus == null ? WatchlistStatus.Reviewed : item.WatchlistStatus,
                            PercentageOfOwnership = Decimal.TryParse(companyShareHolder?.EffectiveShareholding, out decimal result) ? result : 0
                        });
                    }
                    else if (item.OwnershipStrucureType == OwnershipStrucureType.IndividualShareholder)
                    {
                        var individualShareHolder = await businessProfileRepository.GetIndividualShareholderByTableIDAsync(item.TableId);

                        screeningResults.Add(new WatchListReview
                        {
                            ownershipStrucureType = OwnershipStrucureType.IndividualShareholder,
                            isPEP = watchlist == null ? (item.PEPCount > 0) : (watchlist.IsTrueHitPEP.HasValue ? (bool)watchlist.IsTrueHitPEP : false),
                            isAdverseMedia = watchlist == null ? (item.AdverseMediaCount > 0) : (watchlist.EnforcementActions == EnforcementActions.Yes ? true : false),
                            isWatchList = watchlist == null ? false : true,
                            watchlistStatus = watchlist == null ? null : item.WatchlistStatus == null ? WatchlistStatus.Reviewed : item.WatchlistStatus,
                            PercentageOfOwnership = Decimal.TryParse(individualShareHolder?.EffectiveShareholding, out decimal result) ? result : 0
                        });
                    }
                    else if (item.OwnershipStrucureType == OwnershipStrucureType.CompanyLegalEntity)
                    {
                        var companyLegalEntity = await businessProfileRepository.GetCompanyLegalEntityByTableIDAsync(item.TableId);

                        screeningResults.Add(new WatchListReview
                        {
                            ownershipStrucureType = OwnershipStrucureType.CompanyLegalEntity,
                            isPEP = watchlist == null ? (item.PEPCount > 0) : (watchlist.IsTrueHitPEP.HasValue ? (bool)watchlist.IsTrueHitPEP : false),
                            isAdverseMedia = watchlist == null ? (item.AdverseMediaCount > 0) : (watchlist.EnforcementActions == EnforcementActions.Yes ? true : false),
                            isWatchList = watchlist == null ? false : true,
                            watchlistStatus = watchlist == null ? null : item.WatchlistStatus == null ? WatchlistStatus.Reviewed : item.WatchlistStatus,
                            PercentageOfOwnership = Decimal.TryParse(companyLegalEntity?.EffectiveShareholding, out decimal result) ? result : 0
                        });
                    }
                    else if (item.OwnershipStrucureType == OwnershipStrucureType.IndividualLegalEntity)
                    {
                        var individualLegalEntity = await businessProfileRepository.GetIndividualLegalEntityByTableIDAsync(item.TableId);

                        screeningResults.Add(new WatchListReview
                        {
                            ownershipStrucureType = OwnershipStrucureType.IndividualLegalEntity,
                            isPEP = watchlist == null ? (item.PEPCount > 0) : (watchlist.IsTrueHitPEP.HasValue ? (bool)watchlist.IsTrueHitPEP : false),
                            isAdverseMedia = watchlist == null ? (item.AdverseMediaCount > 0) : (watchlist.EnforcementActions == EnforcementActions.Yes ? true : false),
                            isWatchList = watchlist == null ? false : true,
                            watchlistStatus = watchlist == null ? null : item.WatchlistStatus == null ? WatchlistStatus.Reviewed : item.WatchlistStatus,
                            PercentageOfOwnership = Decimal.TryParse(individualLegalEntity?.EffectiveShareholding, out decimal result) ? result : 0
                        });
                    }
                }

                if (!screeningResults.Any(x => x.isWatchList && x.watchlistStatus != WatchlistStatus.Reviewed))
                {
                    bool isPEP = false;
                    bool isEnforcementAction = false;

                    double value = 1.0 / 3.0;

                    // Checking for PEPCount ShareHolders or Legal Entities >= 25%, Any PrimaryOfficer or AuthorisedPerson isPEP
                    if ((screeningResults.Where(x => (x.ownershipStrucureType == OwnershipStrucureType.CompanyShareholder || x.ownershipStrucureType == OwnershipStrucureType.IndividualShareholder) && x.isPEP).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.ownershipStrucureType == OwnershipStrucureType.CompanyLegalEntity || x.ownershipStrucureType == OwnershipStrucureType.IndividualLegalEntity) && x.isPEP).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.ownershipStrucureType == OwnershipStrucureType.PrimaryOfficer || x.ownershipStrucureType == OwnershipStrucureType.AuthorisedPerson)).Any(y => y.isPEP)))
                    {
                        isPEP = true;
                    }

                    // Checking for Enforcement Action (isAdverseMedia) ShareHolders or Legal Entities >= 25%, Any PrimaryOfficer or AuthorisedPerson Enforcement Action
                    if ((screeningResults.Where(x => (x.ownershipStrucureType == OwnershipStrucureType.CompanyShareholder || x.ownershipStrucureType == OwnershipStrucureType.IndividualShareholder) && (x.isAdverseMedia)).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.ownershipStrucureType == OwnershipStrucureType.CompanyLegalEntity || x.ownershipStrucureType == OwnershipStrucureType.IndividualLegalEntity) && (x.isAdverseMedia)).Sum(y => y.PercentageOfOwnership) >= 25) ||
                       (screeningResults.Where(x => (x.ownershipStrucureType == OwnershipStrucureType.PrimaryOfficer || x.ownershipStrucureType == OwnershipStrucureType.AuthorisedPerson)).Any(y => y.isAdverseMedia)))
                    {
                        isEnforcementAction = true;
                    }

                    if (screeningResults.Where(x => x.ownershipStrucureType == OwnershipStrucureType.BoardOfDirector).Count() > 0)
                    {
                        double bodPEP = screeningResults.Where(x => x.ownershipStrucureType == OwnershipStrucureType.BoardOfDirector && x.isPEP).Count() / screeningResults.Where(x => x.ownershipStrucureType == OwnershipStrucureType.BoardOfDirector).Count();
                        double bodEnforcementAction = screeningResults.Where(x => x.ownershipStrucureType == OwnershipStrucureType.BoardOfDirector && x.isAdverseMedia).Count() / screeningResults.Where(x => x.ownershipStrucureType == OwnershipStrucureType.BoardOfDirector).Count();

                        if (bodPEP >= value)
                        {
                            isPEP = true;
                        }

                        if (bodEnforcementAction >= value)
                        {
                            isEnforcementAction = true;
                        }
                    }

                    var partnerSubscription = await partnerRepository.GetSalesPartnerSubscriptionListAsync(partner.Id);

                    List<RBAPartnerSubscrption> groupPartnerSubs = new List<RBAPartnerSubscrption>();


                    // Currently only call RBA for TB + Entities
                    groupPartnerSubs = partnerSubscription.Where(x => x.Solution == Solution.Business)
                        .GroupBy(x => new { x.Solution, x.TrangloEntity })
                        .Select(grp => new RBAPartnerSubscrption
                        {
                            Solution = grp.Key.Solution,
                            TrangloEntity = grp.Key.TrangloEntity
                        }).ToList();

                    // PEPCount and EnformentAction both value base on Watch List Review Result
                    await ProcessRequestData(groupPartnerSubs, businessProfile, customerCategory, isPEP, isEnforcementAction);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error processing risk evaluations with watch list for Business Profile code {screeningInput.BusinessProfile.Id}.");
            }
        }

        public async Task ProcessRequestData(List<RBAPartnerSubscrption> groupPartnerSubs, BusinessProfile businessProfile, CustomerType customerCategory, bool isPEP, bool isEnforcementAction)
        {
            // Get the base URL from app settings
            var baseUrl = config["RBAAPI"];

            List<RBA> rbaResultList = new List<RBA>();

            // Get the mapped API value from the dictionary
            if (serviceTypeToApiValue.TryGetValue(businessProfile.ServiceType, out var apiServiceType))
            {
                RiskEvaluationRequest request = new RiskEvaluationRequest();

                string collectionTier = null;

                if (businessProfile.ServiceType == ServiceType.Collection_Anyone || businessProfile.ServiceType == ServiceType.Collection_Ownself)
                {
                    collectionTier = businessProfile.CollectionTier.Name.Replace(" ", "");
                }

                // Construct the complete API URL by appending the route components
                if (customerCategory == CustomerType.Individual)
                {
                    // Create the request object based on screening data
                    request = new IndividualRiskEvaluationRequest
                    {
                        IndustrySector = businessProfile.BusinessNature.Id,
                        Nationality = businessProfile.NationalityMeta?.CountryISO2,
                        CollectionTier = collectionTier,
                        IsPEP = isPEP,
                        EnforcementActionTakenByRegulator = isEnforcementAction
                    };
                }
                else
                {
                    // Create the request object based on screening data
                    request = new CorporateRiskEvaluationRequest
                    {
                        IncorporationDate = businessProfile.DateOfIncorporation,
                        IncorporationCountry = businessProfile.CompanyRegisteredCountryMeta?.CountryISO2,
                        IncorporationType = businessProfile.IncorporationCompanyTypeCode,
                        IndustrySector = businessProfile.BusinessNature.Id,
                        CustomerCategory = customerCategory.Id,
                        CollectionTier = collectionTier,
                        IsPEP = isPEP,
                        EnforcementActionTakenByRegulator = isEnforcementAction
                    };
                }

                // Need to Call RBA for different entities
                foreach (var partnerSub in groupPartnerSubs)
                {
                    string apiUrl = string.Empty;

                    // Construct the complete API URL by appending the route components
                    if (customerCategory == CustomerType.Individual)
                    {
                        apiUrl = $"{baseUrl}/risk-evaluations/{partnerSub.TrangloEntity}/tranglo-business/individual/{apiServiceType}";
                    }
                    else
                    {
                        apiUrl = $"{baseUrl}/risk-evaluations/{partnerSub.TrangloEntity}/tranglo-business/corporate/{apiServiceType}";
                    }

                    string rbaJson = JsonConvert.SerializeObject(request);

                    // Make the API request
                    var response = await MakeApiRequestAsync(apiUrl, rbaJson, apiServiceType, partnerSub.TrangloEntity);

                    // Check the response and read the content if needed
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        var riskEvaluationResponse = JsonConvert.DeserializeObject<RiskEvaluationResponse>(result);

                        RBA processedRBAResult = await RBAResultMapping(riskEvaluationResponse, businessProfile.Id, partnerSub.Solution, partnerSub.TrangloEntity);

                        rbaResultList.Add(processedRBAResult);
                    }
                    else
                    {
                        // POST RBA failed
                        logger.LogError($"POST RBA Failed for code: {businessProfile.Id}");
                    }
                }

                // Save RBA Results
                if (rbaResultList.Count > 0)
                {
                    RBAScreeningInput rbaScreeningInput = new RBAScreeningInput(businessProfile.Id, businessProfile.BusinessNature, collectionTier, businessProfile.NationalityMeta?.CountryISO2,
                        businessProfile.DateOfIncorporation, businessProfile.CompanyRegisteredCountryMeta?.CountryISO2, businessProfile.IncorporationCompanyTypeCode, customerCategory.Id, isPEP, isEnforcementAction, rbaResultList);

                    var result = await rbaRepository.SaveRBAScreeningInputAsync(rbaScreeningInput);
                }
            }
            else
            {
                // Handle the case for unknown ServiceType values
                throw new ArgumentException("Unknown ServiceType value.");
            }
        }

        private async Task<RBA> RBAResultMapping(RiskEvaluationResponse rbaResponse, int businessProfileId, Solution solution, string trangloEntity)
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

        private async Task<HttpResponseMessage> MakeApiRequestAsync(string apiUrl, string rbaJson, string apiServiceType, string trangloEntity)
        {
            // Create an HttpClient instance
            using (var httpClient = new HttpClient())
            {
                // Set the headers for TrangloEntity and ServiceType
                //httpClient.DefaultRequestHeaders.Add("TrangloEntity", trangloEntity);
                //httpClient.DefaultRequestHeaders.Add("ServiceType", apiServiceType); // Use the mapped value

                var content = new StringContent(rbaJson, Encoding.UTF8, "application/json");

                // Send an HTTP GET request
                var response = await httpClient.PostAsync(apiUrl, content);

                return response;
            }
        }


        private readonly Dictionary<ServiceType, string> serviceTypeToApiValue = new Dictionary<ServiceType, string>
        {
            { ServiceType.Collection_Anyone, "CollectionAndPayout" },
            { ServiceType.Collection_Ownself, "CollectionAndPayout" },
            { ServiceType.Collection_Payout, "Payout" },
        };

        public static Solution MapPlatformToSolution(string platformDescription)
        {
            switch (platformDescription)
            {
                case "Tranglo Connect":
                    return Solution.Connect;
                case "Tranglo Business":
                    return Solution.Business;
                // Handle other platform descriptions or provide a default value if needed
                default:
                    return Solution.Undefined;
            }
        }

        public static PartnerType MapPartnerTypeToEnum(string partnerTypeDescription)
        {
            switch (partnerTypeDescription)
            {
                case "Sales Partner":
                    return PartnerType.Sales_Partner;
                case "Supply Partner":
                    return PartnerType.Supply_Partner;
                // Handle other partner type descriptions or provide a default value if needed
                default:
                    return null;
            }
        }

        public static ScreeningEntityType MapEntityTypeToEnum(string entityTypeDescription)
        {
            switch (entityTypeDescription)
            {
                case "Individual":
                    return ScreeningEntityType.Individual;
                case "Corporate":
                    return ScreeningEntityType.Natural;
                // Handle other entity type descriptions or provide a default value if needed
                default:
                    return null;
            }
        }

        private short? CalculateYearOfBirth(DateTime? dateOfBirth)
        {
            if (dateOfBirth.HasValue)
            {
                return (short)dateOfBirth.Value.Year;
            }
            else
            {
                return null; // Return null if dateOfBirth is null
            }
        }
    }
}

