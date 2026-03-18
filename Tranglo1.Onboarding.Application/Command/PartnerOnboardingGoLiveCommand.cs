using CSharpFunctionalExtensions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerOnboarding;

namespace Tranglo1.Onboarding.Application.Command
{
    internal class PartnerOnboardingGoLiveCommand : BaseCommand<Result<PartnerOnboardingGoLiveOutputDTO>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }

        public override Task<string> GetAuditLogAsync(Result<PartnerOnboardingGoLiveOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Partner Onboarding Go Live for Partner Id: [{this.PartnerCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }



        internal class PartnerOnboardingGoLiveCommandHandler : IRequestHandler<PartnerOnboardingGoLiveCommand, Result<PartnerOnboardingGoLiveOutputDTO>>
        {
            private readonly IPartnerRepository _partnerRepository;
            private readonly IBusinessProfileRepository _businessProfileService;


            public PartnerOnboardingGoLiveCommandHandler(IBusinessProfileRepository businessProfileService, IPartnerRepository partnerRepository)
            {
                _businessProfileService = businessProfileService;
                _partnerRepository = partnerRepository;
            }

            public async Task<Result<PartnerOnboardingGoLiveOutputDTO>> Handle(PartnerOnboardingGoLiveCommand request, CancellationToken cancellationToken)
            {
                var getPartnerOnboarding = await _partnerRepository.GetPartnerOnboardingEventAsync(request.PartnerSubscriptionCode);
                //if (getPartnerOnboarding != null)
                //{
                //    return Result.Failure<PartnerOnboardingGoLiveOutputDTO>($"Partner Onboarding Event with Partner Subscription Code: {request.PartnerSubscriptionCode} already exists.");
                //}
                var getPartner = await _partnerRepository.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
                if (getPartner == null)
                {
                    return Result.Failure<PartnerOnboardingGoLiveOutputDTO>($"Partner Registration with Partner ID: {request.PartnerCode} is null.");
                }
                var getPartnerSubscription = await _partnerRepository.GetPartnerSubscriptionByCodeAsync(request.PartnerSubscriptionCode);
                if (getPartnerSubscription == null)
                {
                    return Result.Failure<PartnerOnboardingGoLiveOutputDTO>($"Partner Registration with Subscription Code: {request.PartnerSubscriptionCode} is null.");
                }

                var getProfile = await _businessProfileService.GetBusinessProfileByCodeAsync(getPartner.BusinessProfileCode);
                if (getProfile == null)
                {
                    return Result.Failure<PartnerOnboardingGoLiveOutputDTO>($"Business Profile with Code: {getPartner.BusinessProfileCode} does not exist ");
                }

                var trangloEntity = getPartnerSubscription.TrangloEntity;
                var solutionCode = getPartnerSubscription.Solution.Id;
                var partnerTypeCode = getPartnerSubscription.PartnerType.Id;
                var settlementCurrencyCode = getPartnerSubscription.SettlementCurrencyCode;
                var businessNatureDescription = getProfile.BusinessNature.Name;
                getPartnerOnboarding.EnvironmentCode = Environment.Production.Id;
                var productionPartnerProfileChangedEvent = getPartnerOnboarding.EnvironmentCode;

                if (getProfile.BusinessNature == BusinessNature.Other)
                {
                    businessNatureDescription = getProfile.ForOthers;
                }

                var partnerOnboarding = new PartnerProfileChangedEvent(trangloEntity, solutionCode, partnerTypeCode, getPartner.Id, getPartner.PartnerId, request.PartnerSubscriptionCode, settlementCurrencyCode, getProfile.CompanyRegisteredCountryMeta.CountryISO2,
                     getProfile.CompanyName, getProfile.CompanyRegistrationNo, getProfile.CompanyRegisteredAddress, getProfile.IDExpiryDate, getProfile.DateOfBirth, getProfile.ContactNumber, getPartner.Email,
                                getProfile.CompanyRegisteredZipCodePostCode, getProfile.BusinessNature.Id, businessNatureDescription,
                                productionPartnerProfileChangedEvent, getProfile.DateOfIncorporation,
                                getProfile.IncorporationCompanyTypeCode, getPartner.CustomerTypeCode, getProfile.CollectionTier.Id, getProfile.Id);
                
                var createPartnerOnboarding = await _partnerRepository.AddPartnerOnboardingCreationEventAsync(partnerOnboarding);
                if (createPartnerOnboarding.IsFailure)
                {
                    return Result.Failure<PartnerOnboardingGoLiveOutputDTO>($"Unable to create Partner Onboarding Event");
                
                }

                 if (getPartnerSubscription.Environment != Environment.Production) //To prevent entity tracking issue
                 {
                     // Partner Environment change to Production once GOLive
                     getPartnerSubscription.SetConnectEnvironment();
                     await _partnerRepository.UpdatePartnerSubscriptionsAsync(getPartnerSubscription);
                 }
                

                var outputDTO = new PartnerOnboardingGoLiveOutputDTO()
                {
                    PartnerCode = getPartner.Id,
                    Status = true
                };

                return Result.Success(outputDTO);
            }
        }
    }
}