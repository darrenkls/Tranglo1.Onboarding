using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.LegalEntitiy;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
    [Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    internal class SaveUltimateBeneficialOwnerCommand : BaseCommand<Result<IEnumerable<LegalEntitiyOutputDTO>>>
    {
        public IEnumerable<LegalEntitiyInputDTO> LegalEntities;
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public Guid? LegalEntityConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<IEnumerable<LegalEntitiyOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Legal Entities for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveUltimateBeneficialOwnerCommandHandler : IRequestHandler<SaveUltimateBeneficialOwnerCommand, Result<IEnumerable<LegalEntitiyOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveUltimateBeneficialOwnerCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IConfiguration _config;

        public SaveUltimateBeneficialOwnerCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                ILogger<SaveUltimateBeneficialOwnerCommandHandler> logger,
                IMapper mapper,
                PartnerService partnerService,
                IBusinessProfileRepository businessProfileRepository,
                IScreeningRepository screeningRepository,
                IConfiguration config
            )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
            _screeningRepository = screeningRepository;
            _config = config;
        }

        public async Task<Result<IEnumerable<LegalEntitiyOutputDTO>>> Handle(SaveUltimateBeneficialOwnerCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            if (businessProfileList.IsFailure)
            {
                return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>("Invalid Business Profile");
            }

            var businessProfile = businessProfileList.Value;
            Result<IEnumerable<LegalEntitiyOutputDTO>> result = new Result<IEnumerable<LegalEntitiyOutputDTO>>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);

            // Handle concurrency
            var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
            {
                var concurrencyCheck = ConcurrencyCheck(request.LegalEntityConcurrencyToken, businessProfile);
                if (concurrencyCheck.Result.IsFailure)
                {
                    return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(concurrencyCheck.Result.Error);
                }
            }


            if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete ||
                kycBusinessReviewResult != null))
            {
                //update
                result = await UpdateLegalEntities(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                        $"Customer user is unable to update for {request.BusinessProfileCode}."
                                        );
                }

                await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode,
                    KYCCategory.Business_Ownership.Id,
                    cancellationToken);
            }

            else if (applicationUser is TrangloStaff &&
                ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                || kycBusinessReviewResult != null)))
            {
                //update
                result = await UpdateLegalEntities(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                        $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                                        );
                }

                //check mandatory fields
                //temporary commenting due to design issue on API causing deadlock
                //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
            }

            else
            {
                return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                         $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                         );
            }

            return result;
        }

        private async Task<Result<IEnumerable<LegalEntitiyOutputDTO>>> UpdateLegalEntities(SaveUltimateBeneficialOwnerCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            try
            {
                var legalEntitiesResult = await _businessProfileService.GetLegalEntityByBusinessProfileCodeAsync(businessProfile);


                IReadOnlyList<LegalEntity> legalEntities =
                    legalEntitiesResult.IsSuccess ? legalEntitiesResult.Value : Enumerable.Empty<IndividualLegalEntity>().ToList().AsReadOnly();


                //Dictionary<string, Country> _CountryList = new Dictionary<string, Country>(StringComparer.OrdinalIgnoreCase);

                foreach (var inputLegalEntity in request.LegalEntities)
                {
                    if (inputLegalEntity.PositionTitle != null && inputLegalEntity.PositionTitle.Length > 50)
                    {
                        return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                             $"Position Title cannot more than 50 characters. "
                                             );
                    }

                    if (inputLegalEntity.TitleOthers != null && inputLegalEntity.TitleOthers.Length > 50)
                    {
                        return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                             $"Title cannot be more than 50 characters. "
                                             );
                    }

                    if (inputLegalEntity.LegalEntityCode == 0 || inputLegalEntity.LegalEntityCode == null)
                    {
                        // To Add
                        if (inputLegalEntity.ShareholderTypeCode == ShareholderType.Individual.Id)
                        {
                            var companyName = inputLegalEntity.CompanyName;

                            var gender = await _businessProfileRepository.GetGenderTypeByCode(inputLegalEntity.GenderCode);

                            var idCode = await _businessProfileRepository.GetIDTypeByCode(inputLegalEntity.IdTypeCode);


                            var nationalityCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputLegalEntity.NationalityISO2);

                            var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputLegalEntity.CountryOfResidenceISO2);
                            var titleCode = await _businessProfileRepository.GetTitleTypeByCode(inputLegalEntity.TitleCode);
                            var dateOfBirth = inputLegalEntity.DateOfBirth;

                            IndividualLegalEntity individual =
                                new IndividualLegalEntity(businessProfile, inputLegalEntity.CompanyName,
                                                             inputLegalEntity.EffectiveShareholding, inputLegalEntity.ResidentialAddress, inputLegalEntity.ZipCodePostCode,
                                                             idCode, inputLegalEntity.IDNumber, nationalityCountry, gender, dateOfBirth,
                                                            inputLegalEntity.IDExpiryDate, countryOfResidence, inputLegalEntity.PositionTitle, titleCode, inputLegalEntity.TitleOthers);

                            await _businessProfileService.AddLegalEntityAsync(businessProfile, individual);
                        }

                        else if (inputLegalEntity.ShareholderTypeCode == ShareholderType.Company.Id)
                        {
                            var country = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputLegalEntity.CountryISO2);
                            var companyName = inputLegalEntity.CompanyName;
                            var regNo = inputLegalEntity.CompanyRegNo;
                            var dateOfIncorpo = inputLegalEntity.DateOfIncorporation;


                            CompanyLegalEntity company =
                              new CompanyLegalEntity(businessProfile, companyName, regNo,
                                                        inputLegalEntity.NameOfSharesAboveTenPercent, inputLegalEntity.EffectiveShareholding, dateOfIncorpo, country);

                            await _businessProfileService.AddLegalEntityAsync(businessProfile, company);
                        }

                        else
                        {
                            return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                $"Invalid ShareholderTypeCode[{inputLegalEntity.ShareholderTypeCode}].");
                        }
                    }

                    //To Update
                    else if (!inputLegalEntity.IsDeleted)
                    {
                        var _ExistingLegalEntities = legalEntities
                            .Where(x => x.Id == inputLegalEntity.LegalEntityCode)
                            .FirstOrDefault();

                        if (_ExistingLegalEntities == null)
                        {
                            return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                $"Invalid Shareholder Code[{inputLegalEntity.LegalEntityCode}].");
                        }


                        if (inputLegalEntity.ShareholderTypeCode == ShareholderType.Individual.Id && _ExistingLegalEntities is IndividualLegalEntity _ExistingIndividualLegalEntity)
                        {
                            var companyName = inputLegalEntity.CompanyName;
                            var gender = await _businessProfileRepository.GetGenderTypeByCode(inputLegalEntity.GenderCode);
                            var idCode = await _businessProfileRepository.GetIDTypeByCode(inputLegalEntity.IdTypeCode);
                            var nationality = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputLegalEntity.NationalityISO2);
                            var dateOfBirth = inputLegalEntity.DateOfBirth;
                            var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputLegalEntity.CountryOfResidenceISO2);
                            var titleCode = await _businessProfileRepository.GetTitleTypeByCode(inputLegalEntity.TitleCode);

                            _ExistingIndividualLegalEntity.Nationality = nationality ?? _ExistingIndividualLegalEntity.Nationality;
                            //_ExistingIndividualLegalEntity.CountryOfResidence = countryOfResidence ?? _ExistingIndividualLegalEntity.CountryOfResidence;
                            _ExistingIndividualLegalEntity.CountryOfResidence = countryOfResidence;
                            _ExistingIndividualLegalEntity.CompanyName = companyName ?? _ExistingIndividualLegalEntity.CompanyName;
                            _ExistingIndividualLegalEntity.Gender = gender ?? _ExistingIndividualLegalEntity.Gender;
                            //_ExistingIndividualLegalEntity.IDType = idCode ?? _ExistingIndividualLegalEntity.IDType;
                            _ExistingIndividualLegalEntity.IDType = idCode;
                            _ExistingIndividualLegalEntity.DateOfBirth = dateOfBirth ?? _ExistingIndividualLegalEntity.DateOfBirth;
                            _ExistingIndividualLegalEntity.EffectiveShareholding = inputLegalEntity.EffectiveShareholding ?? _ExistingIndividualLegalEntity.EffectiveShareholding;
                            //_ExistingIndividualLegalEntity.IDNumber = inputLegalEntity.IDNumber ?? _ExistingIndividualLegalEntity.IDNumber;
                            _ExistingIndividualLegalEntity.IDNumber = inputLegalEntity.IDNumber;
                            //_ExistingIndividualLegalEntity.IDExpiryDate = inputLegalEntity.IDExpiryDate ?? _ExistingIndividualLegalEntity.IDExpiryDate;
                            _ExistingIndividualLegalEntity.IDExpiryDate = inputLegalEntity.IDExpiryDate;
                            _ExistingIndividualLegalEntity.ResidentialAddress = inputLegalEntity.ResidentialAddress ?? _ExistingIndividualLegalEntity.ResidentialAddress;
                            _ExistingIndividualLegalEntity.ZipCodePostCode = inputLegalEntity.ZipCodePostCode ?? _ExistingIndividualLegalEntity.ZipCodePostCode;
                            _ExistingIndividualLegalEntity.PositionTitle = inputLegalEntity.PositionTitle ?? _ExistingIndividualLegalEntity.PositionTitle;
                            _ExistingIndividualLegalEntity.Title = titleCode;
                            _ExistingIndividualLegalEntity.TitleOthers = inputLegalEntity.TitleOthers;


                            await _businessProfileService.UpdateLegalEntityAsync(businessProfile, _ExistingIndividualLegalEntity, cancellationToken);

                        }

                        else if (inputLegalEntity.ShareholderTypeCode == ShareholderType.Company.Id && _ExistingLegalEntities is CompanyLegalEntity _ExistingCompanyLegalEntity)
                        {
                            var country = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputLegalEntity.CountryISO2);

                            var companyName = inputLegalEntity.CompanyName;
                            var regNo = inputLegalEntity.CompanyRegNo;
                            var dateOfIncorpo = inputLegalEntity.DateOfIncorporation;


                            _ExistingCompanyLegalEntity.Country = country;

                            _ExistingCompanyLegalEntity.DateOfIncorporation = dateOfIncorpo ?? _ExistingCompanyLegalEntity.DateOfIncorporation;
                            _ExistingCompanyLegalEntity.CompanyName = companyName ?? _ExistingCompanyLegalEntity.CompanyName;
                            _ExistingCompanyLegalEntity.CompanyRegNo = regNo ?? _ExistingCompanyLegalEntity.CompanyRegNo;
                            _ExistingCompanyLegalEntity.NameOfSharesAboveTenPercent = inputLegalEntity.NameOfSharesAboveTenPercent ?? _ExistingCompanyLegalEntity.NameOfSharesAboveTenPercent;
                            _ExistingCompanyLegalEntity.EffectiveShareholding = inputLegalEntity.EffectiveShareholding ?? _ExistingCompanyLegalEntity.EffectiveShareholding;

                            await _businessProfileService.UpdateLegalEntityAsync(businessProfile, _ExistingCompanyLegalEntity, cancellationToken);

                        }

                        else
                        {
                            return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(
                                $"Invalid Shareholder TypeCode[{inputLegalEntity.ShareholderTypeCode}].");
                        }
                    }
                }

                // TO Delete
                var deletedLegalEntity = from existing in legalEntities
                                         let fromInput = request.LegalEntities
                                         .FirstOrDefault(input =>
                                         input.IsDeleted &&
                                         input.LegalEntityCode.HasValue &&
                                         input.LegalEntityCode.Value == existing.Id)
                                         where fromInput?.LegalEntityCode == existing?.Id
                                         select existing;

                if (deletedLegalEntity.Any())
                {
                    List<long> deletedLegalEntityIds = deletedLegalEntity.Select(existing => existing.Id).ToList();

                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedLegalEntityIds, businessProfile.Id);
                    await _businessProfileService.DeleteLegalEntityAsync(businessProfile, deletedLegalEntity, cancellationToken);
                }

                legalEntitiesResult = await _businessProfileService.GetLegalEntityByBusinessProfileCodeAsync(businessProfile);
                var _isUltimateBeneficialOwnerCompleted = await _businessProfileService.IsOwnershipLegalEntityCompleted(businessProfile.Id);

                // Check if any Benficial Owners remain after updates or deletions
                var remainingBeneficialOwners = await _businessProfileService.GetLegalEntityByBusinessProfileCodeAsync(businessProfile);

                // After updates or deletions
                if (remainingBeneficialOwners.IsSuccess && !remainingBeneficialOwners.Value.Any())
                {
                    // All Benficial Owners are deleted, set the concurrency token to null
                    businessProfile.LegalEntityConcurrencyToken = null;
                }
                else
                {
                    // Update the concurrency token since some  Benficial Owners are still present
                    businessProfile.LegalEntityConcurrencyToken = Guid.NewGuid();
                }

                // Update the business profile
                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success<IEnumerable<LegalEntitiyOutputDTO>>(legalEntitiesResult.Value.Select(s => new LegalEntitiyOutputDTO()
                {
                    CompanyName = s.CompanyName,
                    CompanyRegNo = s.CompanyRegNo,
                    NameOfSharesAboveTenPercent = s.NameOfSharesAboveTenPercent,
                    EffectiveShareholding = s.EffectiveShareholding,
                    // Name = s is IndividualLegalEntity indvidualLegalEntity ? indvidualLegalEntity.Name : null,
                    IdTypeCode = s is IndividualLegalEntity individualLegalEntityIDType ? individualLegalEntityIDType.IDType?.Id : null,
                    IDNumber = s is IndividualLegalEntity individualLegalIDNumber ? individualLegalIDNumber.IDNumber : null,
                    LegalEntityCode = s.Id,
                    ShareholderTypeCode = s is IndividualLegalEntity ? 1 : 2,
                    NationalityISO2 = s is IndividualLegalEntity individualLegalEntityNationality ? individualLegalEntityNationality.Nationality?.CountryISO2 : null,
                    GenderCode = s is IndividualLegalEntity individualLegalEntityGender ? individualLegalEntityGender.Gender?.Id : null,
                    DateOfBirth = s is IndividualLegalEntity individualLegalEntityDateOfBirth ? individualLegalEntityDateOfBirth.DateOfBirth : null,
                    CountryISO2 = s is CompanyLegalEntity companyLegalEntityCountry ? companyLegalEntityCountry.Country?.CountryISO2 : null,
                    DateOfIncorporation = s is CompanyLegalEntity companyLegalEntityDateOfIncorporation ? companyLegalEntityDateOfIncorporation.DateOfIncorporation : null,
                    IDExpiryDate = s is IndividualLegalEntity individualLegalIDExpiryDate ? individualLegalIDExpiryDate.IDExpiryDate : null,
                    CountryOfResidenceISO2 = s is IndividualLegalEntity individualLegalCountryOfResidence ? individualLegalCountryOfResidence.CountryOfResidence?.CountryISO2 : null,
                    PositionTitle = s is IndividualLegalEntity individualLegalEntityPositionTitle ? individualLegalEntityPositionTitle.PositionTitle : null,
                    ResidentialAddress = s is IndividualLegalEntity individualLegalResidentialAddress ? individualLegalResidentialAddress.ResidentialAddress : null,
                    ZipCodePostCode = s is IndividualLegalEntity individualZipCodePostCode ? individualZipCodePostCode.ZipCodePostCode : null,
                    isCompleted = _isUltimateBeneficialOwnerCompleted,
                    LegalEntityConcurrencyToken = businessProfile.LegalEntityConcurrencyToken,
                    TitleCode = s is IndividualLegalEntity individualLegalEntityTitle ? individualLegalEntityTitle.Title?.Id : null,
                    TitleOthers = s is IndividualLegalEntity individualLegalEntityTitleOthers ? individualLegalEntityTitleOthers.TitleOthers : null,

                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString(), "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<IEnumerable<LegalEntitiyOutputDTO>>(ex.ToString());
            }
        }

        private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.LegalEntityConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.LegalEntityConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<LegalEntitiyOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.LegalEntityConcurrencyToken = Guid.NewGuid();
                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<LegalEntitiyOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
            }
        }

        private async Task MarkKYCSummaryNotificationsAsReadAsync(int businessProfileCode,
            long kycCategoryCode,
            CancellationToken cancellationToken)
        {
            try
            {
                Specification<KYCSummaryFeedbackNotification> specification = new UnreadKYCSummaryFeedbackNotificationByBusinessProfileAndKYCCategory(
                    businessProfileCode,
                    kycCategoryCode
                );

                await _businessProfileRepository.UpdateKYCSummaryFeedbackNotificationsAsReadByCategoryAsync(specification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{0}]", nameof(SaveUltimateBeneficialOwnerCommandHandler));
            }
        }
    }
}
