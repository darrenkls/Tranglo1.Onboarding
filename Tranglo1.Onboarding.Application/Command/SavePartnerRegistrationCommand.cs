using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerRegistration, UACAction.Edit)]
    [Permission(Permission.RegisterNewPartner.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.RegisterNewPartner.Action_View_Code }
        )]
    internal class SavePartnerRegistrationCommand : BaseCommand<Result<PartnerRegistrationOutputDTO>>
    {
        public IEnumerable<PartnerRegistrationInputDTO> PartnerRegistration;
        public string LoginId { get; set; }
        public long? PartnerCode { get; set; }
        public string RegisteredCompanyName { get; set; }
        public string TradeName { get; set; }
        public string CompanyRegisteredNo { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string IMID { get; set; }
        public long? BusinessNature { get; set; }
        public string ZipCodePostCode { get; set; }
        public string CountryISO2 { get; set; }
        public string Entity { get; set; }
        //public long? PartnerType { get; set; }
        public string CompanyAddress { get; set; }
        //public long? Solution { get; set; }
        //public string Currency { get; set; }
        public string TimeZone { get; set; }
        public string Agent { get; set; }
        //public long? PricePackageCode { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        public string UserBearerToken { get; set; }
        public string PartnerName { get; set; }
        //public bool DisplayDefaultPackage { get; set; }
        public string ContactPersonName { get; set; }
        public string ForOthers { get; set; }

        //Phase 3 Changes
        public long? CustomerTypeCode { get; set; }
        public string FullName { get; set; }
        public string PersonInChargeName { get; set; }
        public string AliasName { get; set; }
        public string NationalityISO2 { get; set; }
        public long? RelationshipTieUpCode { get; set; }
        public string FormerRegisteredCompanyName { get; set; }


        public List<long?> SolutionTypeCode { get; set; }
        public long? TitleCode { get; set; }
        public string TitleOthers { get; set; }


        public override Task<string> GetAuditLogAsync(Result<PartnerRegistrationOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Registered a new partner";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SavePartnerRegistrationCommandHandler : IRequestHandler<SavePartnerRegistrationCommand, Result<PartnerRegistrationOutputDTO>>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly BusinessProfileService _businessProfileService;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ILogger<SaveParentHoldingCompanyCommandHandler> _logger;

        private class ApiResponse
        {
            public string Detail { get; set; }
        }
        public SavePartnerRegistrationCommandHandler(
              IPartnerRepository partnerRepository,
              BusinessProfileService businessProfileService,
              IApplicationUserRepository applicationUserRepository,
              ILogger<SaveParentHoldingCompanyCommandHandler> logger
          )
        {
            _partnerRepository = partnerRepository;
            _applicationUserRepository = applicationUserRepository;
            _businessProfileService = businessProfileService;
            _logger = logger;
        }

        public async Task<Result<PartnerRegistrationOutputDTO>> Handle(SavePartnerRegistrationCommand request, CancellationToken cancellationToken)
        {
            List<long?> solutionListings = new List<long?>(request.SolutionTypeCode);

            var email = string.IsNullOrWhiteSpace(request.Email) ? null : Email.Create(request.Email).Value;
            var nature = Enumeration.FindById<BusinessNature>(request.BusinessNature.GetValueOrDefault());
            var country = CountryMeta.GetCountryByISO2Async(request.CountryISO2);
            var nationality = CountryMeta.GetCountryByISO2Async(request.NationalityISO2);
            var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(request.CustomerTypeCode);
            var relationship = await _partnerRepository.GetRelationshipTieUpByCodeAsync(request.RelationshipTieUpCode);
            var titleCode = Enumeration.FindById<Title>(request.TitleCode.GetValueOrDefault());

            if (request.TitleOthers != null && request.TitleOthers.Length > 50)
            {
                return Result.Failure<PartnerRegistrationOutputDTO>(
                                     $"Title cannot be more than 50 characters."
                                     );
            }

            //duplicate checking on new partner name
            var _isExistingPartnerName = await _businessProfileService.CheckIsExistingCompanyNameAsync(request.PartnerName);
            if (_isExistingPartnerName.isInUsed)
            {
                return Result.Failure<PartnerRegistrationOutputDTO>(
                    $"Partner Name Already Exist."
                    );
            }
            //Contact
            Result<ContactNumber> createContactNumber = string.IsNullOrWhiteSpace(request.ContactNumber) || string.IsNullOrWhiteSpace(request.DialCode) ? null : ContactNumber.Create(request.DialCode, request.ContactNumberCountryISO2, request.ContactNumber);
            if (createContactNumber.IsFailure)
            {
                return Result.Failure<PartnerRegistrationOutputDTO>(
                    $"Create contact number failed for {request.ContactNumber}. {createContactNumber.Error}"
                    );
            }
            //Get User input for Tranglo Staff Agent
            var ApplicationUser = await _applicationUserRepository.GetApplicationUserByLoginId(request.Agent);
            if (ApplicationUser == null)
            {
                return Result.Failure<PartnerRegistrationOutputDTO>(
                    $"Application User {request.Agent} Not Exist."
                    );
            }
            var trangloStaffAgentName = ApplicationUser.LoginId;

            if (customerType != CustomerType.Individual)
            {
                if (request.RegisteredCompanyName is null)
                {
                    return Result.Failure<PartnerRegistrationOutputDTO>(
                        $"Company Registered Name is required."
                        );
                }
            }

            if (customerType == CustomerType.Individual)
            {
                if (nationality is null)
                {
                    return Result.Failure<PartnerRegistrationOutputDTO>(
                        $"Nationality is required."
                        );
                }
            }

            //Define Collection Tier for Tranglo Business Customer Type
            CollectionTier collectionTier = new CollectionTier();
            if (customerType == CustomerType.Individual || customerType == CustomerType.Corporate_Normal_Corporate || customerType == CustomerType.Corporate_Mass_Payout)
            {
                collectionTier = CollectionTier.Tier_1;
            }
            else if (customerType == CustomerType.Corporate_Cryptocurrency_Exchange || customerType == CustomerType.Remittance_Partner)
            {
                collectionTier = CollectionTier.Tier_3;
            }

            foreach (var solution in solutionListings)
            {
                if (solutionListings.Count > 1)
                {
                    continue;
                }
                else if (solution == Solution.Connect.Id)
                {
                    if (country is null)
                    {
                        return Result.Failure<PartnerRegistrationOutputDTO>(
                            $"Country is required."
                            );
                    }
                }
            }




            var RegisterPartnerResult = await _businessProfileService.EnsurePartnerBusinessProfileAsync(
                request.RegisteredCompanyName,
                request.TradeName,
                nature,
                createContactNumber.Value,
                country,
                email,
                request.IMID,
                request.CompanyAddress,
                request.ZipCodePostCode,
                request.Entity,
                trangloStaffAgentName,
                request.PartnerName,
                request.ContactPersonName,
                request.ForOthers,
                null,
                null,
                customerType?.Id ?? null,
                request.FormerRegisteredCompanyName,
                request.AliasName,
                nationality,
                relationship?.Id ?? null,
                null,
                null,
                null,
                null,
                null,
                collectionTier,
                solutionListings,
                false, // isTncTick
                titleCode,
                request.TitleOthers
                );

            var partner = await _partnerRepository.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(RegisterPartnerResult.Value.Id);

            //Get initial subscription
            var subscription = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partner.Id);
            var initialSubscription = subscription.FirstOrDefault();

            PartnerRegistrationOutputDTO result = new PartnerRegistrationOutputDTO()
            {
                PartnerCode = partner.Id,
                PartnerSubscriptionCode = initialSubscription.Id
            };

            return Result.Success(result);
        }
    }
}










