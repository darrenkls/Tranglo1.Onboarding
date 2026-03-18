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
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.LegalEntitiy;
using Tranglo1.Onboarding.Application.DTO.Shareholder;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
    [Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    internal class SaveShareholderCommand : BaseCommand<Result<IEnumerable<ShareholderOutputDTO>>>
    {
        public IEnumerable<ShareholderInputDTO> Shareholders;
        public int BusinessProfileCode { get; set; }
        public string LoginId { get; set; }

        //Solutions
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public Guid? ShareholderConcurrencyToken { get; set; }


        public override Task<string> GetAuditLogAsync(Result<IEnumerable<ShareholderOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Shareholders for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveShareholderCommandHandler : IRequestHandler<SaveShareholderCommand, Result<IEnumerable<ShareholderOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveShareholderCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IConfiguration _config;

        public SaveShareholderCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                ILogger<SaveShareholderCommandHandler> logger,
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

        public async Task<Result<IEnumerable<ShareholderOutputDTO>>> Handle(SaveShareholderCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            if (businessProfileList.IsFailure)
            {
                return Result.Failure<IEnumerable<ShareholderOutputDTO>>("Invalid Business Profile");
            }


            var businessProfile = businessProfileList.Value;

            // Handle concurrency
            var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
            {
                var concurrencyCheck = ConcurrencyCheck(request.ShareholderConcurrencyToken, businessProfile);
                if (concurrencyCheck.Result.IsFailure)
                {
                    return Result.Failure<IEnumerable<ShareholderOutputDTO>>(concurrencyCheck.Result.Error);
                }
            }

            Result<IEnumerable<ShareholderOutputDTO>> result = new Result<IEnumerable<ShareholderOutputDTO>>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);

            if (ClaimCode.Connect == request.CustomerSolution)
            {
                if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete
                    || kycBusinessReviewResult != null))
                {
                    //update
                    result = await UpdateShareholders(request, businessProfile, cancellationToken);
                    //return error
                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                                            $"Customer user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                                            );
                    }

                }
            }
            else if (ClaimCode.Business == request.CustomerSolution)
            {
                //update
                result = await UpdateShareholders(request, businessProfile, cancellationToken);
                //return error
                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                                        $"Customer user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                                        );
                }

                await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode,
                    KYCCategory.Business_Ownership.Id,
                    cancellationToken);
            }
            else if (Solution.Connect.Id == request.AdminSolution)
            {
                if (applicationUser is TrangloStaff &&
                   ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                   businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                   || kycBusinessReviewResult != null)))
                {
                    //update
                    result = await UpdateShareholders(request, businessProfile, cancellationToken);

                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                                            $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                                            );
                    }

                    //check mandatory fields
                    //temporary commenting due to design issue on API causing deadlock
                    //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
                }
            }
            else if (Solution.Business.Id == request.AdminSolution)
            {
                //update
                result = await UpdateShareholders(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                                        $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                                        );
                }


            }
            else
            {
                return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                                         $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}. "
                                         );
            }

            return result;
        }

        private async Task<Result<IEnumerable<ShareholderOutputDTO>>> UpdateShareholders(SaveShareholderCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            var shareholdersResult = await _businessProfileService.GetShareholderByBusinessProfileCodeAsync(businessProfile);

            IReadOnlyList<Shareholder> shareholders =
                shareholdersResult.IsSuccess ? shareholdersResult.Value : Enumerable.Empty<Shareholder>().ToList().AsReadOnly();

            var legalEntitiesResult = await _businessProfileService.GetLegalEntityByBusinessProfileCodeAsync(businessProfile);

            IReadOnlyList<LegalEntity> legalEntities =
                legalEntitiesResult.IsSuccess ? legalEntitiesResult.Value : Enumerable.Empty<IndividualLegalEntity>().ToList().AsReadOnly();



            //Only pick shareholders that is not deleted
            var shareholdersNotDeleted = request.Shareholders.Where(x => !x.IsDeleted).ToList();

            List<BoardOfDirector> deletedBoardOfDirectors = new List<BoardOfDirector>();
            List<PrimaryOfficer> deletedPrimaryOfficers = new List<PrimaryOfficer>();
            List<AuthorisedPerson> deletedAuthorisedPerson = new List<AuthorisedPerson>();
            List<IndividualLegalEntity> deletedUltimateBeneficialOwners = new List<IndividualLegalEntity>();

            foreach (var inputShareholder in shareholdersNotDeleted)
            {
                if (inputShareholder.PositionTitle != null && inputShareholder.PositionTitle.Length > 50)
                {
                    return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                                         $"Position Title cannot more than 50 characters. "
                                         );
                }

                if (inputShareholder.TitleOthers != null && inputShareholder.TitleOthers.Length > 50)
                {
                    return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                                         $"Title cannot more than 50 characters. "
                                         );
                }

                var shareholder = shareholders
                                .Where(x => x.Id == inputShareholder.ShareholderCode)
                                .FirstOrDefault();

                if (inputShareholder.ShareholderTypeCode == ShareholderType.Individual.Id)
                {
                    var nationality = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputShareholder.NationalityISO2);
                    var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputShareholder.CountryOfResidenceISO2);
                    var gender = await _businessProfileRepository.GetGenderTypeByCode(inputShareholder.GenderCode);
                    var idCode = await _businessProfileRepository.GetIDTypeByCode(inputShareholder.IdTypeCode);
                    var titleCode = await _businessProfileRepository.GetTitleTypeByCode(inputShareholder.TitleCode);

                    IndividualShareholder individual = shareholder != null ? shareholder as IndividualShareholder : null;
                    bool hasOriBOD = shareholder != null && shareholder.BoardOfDirector != null;
                    bool hasOriPrimaryOfficer = shareholder != null && shareholder.PrimaryOfficer != null;
                    bool hasOriAuthorisedPerson = shareholder != null && shareholder.AuthorisedPerson != null;
                    bool hasOriUBO = shareholder != null && shareholder.UltimateBeneficialOwner != null;

                    //new shareholder record
                    if (individual is null) //inputShareholder.ShareholderCode == 0 || inputShareholder.ShareholderCode == null)
                    {
                        individual =
                            new IndividualShareholder(businessProfile, inputShareholder.Name, inputShareholder.DateOfBirth,
                                                        idCode, inputShareholder.IDNumber, inputShareholder.IDExpiryDate, gender,
                                                        inputShareholder.EffectiveShareholding,
                                                        nationality, countryOfResidence, null, null, null, inputShareholder.ResidentialAddress,
                                                        inputShareholder.ZipCodePostCode, inputShareholder.PositionTitle, null, titleCode, inputShareholder.TitleOthers);

                        await _businessProfileService.AddShareholderAsync(businessProfile, individual);
                    }
                    else //update shareholder record
                    {
                        var dateOfBirth = inputShareholder.DateOfBirth;
                        var fullName = inputShareholder.Name;

                        individual.CountryOfResidence = countryOfResidence;
                        individual.Nationality = nationality;
                        individual.Gender = gender;
                        individual.IDType = idCode;
                        individual.Name = fullName;
                        individual.DateOfBirth = dateOfBirth;
                        individual.IDNumber = inputShareholder.IDNumber;
                        individual.IDExpiryDate = inputShareholder.IDExpiryDate;
                        individual.DateOfIncorporation = inputShareholder.DateOfIncorporation;
                        individual.EffectiveShareholding = inputShareholder.EffectiveShareholding;
                        individual.ResidentialAddress = inputShareholder.ResidentialAddress;
                        individual.ZipCodePostCode = inputShareholder.ZipCodePostCode;
                        individual.PositionTitle = inputShareholder.PositionTitle;
                        individual.Title = titleCode;
                        individual.TitleOthers = inputShareholder.TitleOthers;

                        await _businessProfileService.UpdateShareholderAsync(businessProfile, individual, cancellationToken);



                        //Update board of director
                        if (individual.BoardOfDirector?.Id != null)
                        {
                            //var _ExistingBoardOfDirector = await _businessProfileRepository.GetBoardOfDirectorByCodeAsync(individual.BoardOfDirector.Id);
                            var _ExistingBoardOfDirector = individual.BoardOfDirector;

                            _ExistingBoardOfDirector.Name = individual.Name;
                            _ExistingBoardOfDirector.DateOfBirth = individual.DateOfBirth;
                            _ExistingBoardOfDirector.Gender = individual.Gender;
                            _ExistingBoardOfDirector.IDType = individual.IDType;
                            _ExistingBoardOfDirector.IDNumber = individual.IDNumber;
                            _ExistingBoardOfDirector.IDExpiryDate = individual.IDExpiryDate;
                            _ExistingBoardOfDirector.Nationality = individual.Nationality;
                            _ExistingBoardOfDirector.CountryOfResidence = individual.CountryOfResidence;
                            _ExistingBoardOfDirector.ResidentialAddress = individual.ResidentialAddress;
                            _ExistingBoardOfDirector.ZipCodePostCode = individual.ZipCodePostCode;
                            _ExistingBoardOfDirector.PositionTitle = individual.PositionTitle;
                            _ExistingBoardOfDirector.Title = individual.Title;
                            _ExistingBoardOfDirector.TitleOthers = individual.TitleOthers;

                            await _businessProfileRepository.UpdateBoardOfDirectorAsync(_ExistingBoardOfDirector, cancellationToken);
                        }

                        //Update primary officer
                        if (individual.PrimaryOfficer?.Id != null)
                        {
                            var _ExistingPrimaryOfficer = individual.PrimaryOfficer;

                            _ExistingPrimaryOfficer.Name = individual.Name;
                            _ExistingPrimaryOfficer.DateOfBirth = individual.DateOfBirth;
                            _ExistingPrimaryOfficer.Gender = individual.Gender;
                            _ExistingPrimaryOfficer.IDType = individual.IDType;
                            _ExistingPrimaryOfficer.IDNumber = individual.IDNumber;
                            _ExistingPrimaryOfficer.IDExpiryDate = individual.IDExpiryDate;
                            _ExistingPrimaryOfficer.Nationality = individual.Nationality;
                            _ExistingPrimaryOfficer.CountryOfResidence = individual.CountryOfResidence;
                            _ExistingPrimaryOfficer.PositionTitle = individual.PositionTitle;
                            _ExistingPrimaryOfficer.Title = individual.Title;
                            _ExistingPrimaryOfficer.TitleOthers = individual.TitleOthers;

                            await _businessProfileRepository.UpdatePrimaryOfficerAsync(_ExistingPrimaryOfficer, cancellationToken);
                        }

                        //Update authorised person
                        if (individual.AuthorisedPerson?.Id != null)
                        {
                            var _ExistingAuthorisedPerson = individual.AuthorisedPerson;

                            _ExistingAuthorisedPerson.FullName = individual.Name;
                            _ExistingAuthorisedPerson.DateOfBirth = individual.DateOfBirth;
                            _ExistingAuthorisedPerson.IDType = individual.IDType;
                            _ExistingAuthorisedPerson.IDNumber = individual.IDNumber;
                            _ExistingAuthorisedPerson.IDExpiryDate = individual.IDExpiryDate;
                            _ExistingAuthorisedPerson.Nationality = individual.Nationality;
                            _ExistingAuthorisedPerson.CountryOfResidence = individual.CountryOfResidence;
                            _ExistingAuthorisedPerson.ResidentialAddress = individual.ResidentialAddress;
                            _ExistingAuthorisedPerson.ZipCodePostCode = individual.ZipCodePostCode;
                            _ExistingAuthorisedPerson.PositionTitle = individual.PositionTitle;
                            _ExistingAuthorisedPerson.Title = individual.Title;
                            _ExistingAuthorisedPerson.TitleOthers = individual.TitleOthers;

                            await _businessProfileRepository.UpdateAuthorisedPerson(_ExistingAuthorisedPerson, cancellationToken);
                        }

                        if (individual.UltimateBeneficialOwner?.Id != null)
                        {
                            var _ExistingUltimateBeneficialOwner = individual.UltimateBeneficialOwner;

                            _ExistingUltimateBeneficialOwner.CompanyName = individual.Name;
                            _ExistingUltimateBeneficialOwner.IDNumber = individual.IDNumber;
                            _ExistingUltimateBeneficialOwner.DateOfBirth = individual.DateOfBirth;
                            _ExistingUltimateBeneficialOwner.EffectiveShareholding = individual.EffectiveShareholding;
                            _ExistingUltimateBeneficialOwner.IDExpiryDate = individual.IDExpiryDate;
                            _ExistingUltimateBeneficialOwner.Gender = individual.Gender;
                            _ExistingUltimateBeneficialOwner.Nationality = individual.Nationality;
                            _ExistingUltimateBeneficialOwner.ResidentialAddress = individual.ResidentialAddress;
                            _ExistingUltimateBeneficialOwner.ZipCodePostCode = individual.ZipCodePostCode;
                            _ExistingUltimateBeneficialOwner.PositionTitle = individual.PositionTitle;
                            _ExistingUltimateBeneficialOwner.CountryOfResidence = individual.CountryOfResidence;
                            _ExistingUltimateBeneficialOwner.IDType = individual.IDType;
                            _ExistingUltimateBeneficialOwner.IDNumber = individual.IDNumber;
                            _ExistingUltimateBeneficialOwner.Title = individual.Title;
                            _ExistingUltimateBeneficialOwner.TitleOthers = individual.TitleOthers ?? null;

                            await _businessProfileRepository.UpdateLegalEntityAsync(_ExistingUltimateBeneficialOwner, cancellationToken);
                        }
                    }

                    //for new/existing shareholder if toggle is turned on
                    if (!hasOriBOD && inputShareholder.isBoardOfDirector)
                    {
                        BoardOfDirector boardOfDirector =
                            new BoardOfDirector(businessProfile, inputShareholder.Name, inputShareholder.PositionTitle, inputShareholder.DateOfBirth, gender,
                                                    idCode, inputShareholder.IDNumber, inputShareholder.IDExpiryDate,
                                                    nationality, countryOfResidence, inputShareholder.ResidentialAddress, inputShareholder.ZipCodePostCode, titleCode, inputShareholder.TitleOthers);

                        boardOfDirector.Shareholder = individual;
                        individual.BoardOfDirector = boardOfDirector;

                        await _businessProfileRepository.AddBoardOfDirectorAsync(boardOfDirector);
                        await _businessProfileRepository.UpdateShareholderAsync(individual, cancellationToken);
                    }
                    //for new/existing shareholder if toggle is turned on
                    if (!hasOriPrimaryOfficer && inputShareholder.isPrimaryOfficers)
                    {
                        PrimaryOfficer primaryOfficer =
                          new PrimaryOfficer(businessProfile, inputShareholder.Name, inputShareholder.PositionTitle, inputShareholder.DateOfBirth, gender,
                                                  idCode, inputShareholder.IDNumber, inputShareholder.IDExpiryDate,
                                                  nationality, countryOfResidence, inputShareholder.ResidentialAddress, inputShareholder.ZipCodePostCode, titleCode, inputShareholder.TitleOthers);

                        primaryOfficer.Shareholder = individual;
                        individual.PrimaryOfficer = primaryOfficer;

                        await _businessProfileRepository.AddPrimaryOfficerAsync(primaryOfficer);
                        await _businessProfileRepository.UpdateShareholderAsync(individual, cancellationToken);
                    }
                    //for new/existing shareholder if toggle is turned on
                    if (!hasOriAuthorisedPerson && inputShareholder.isAuthorisedPerson)
                    {
                        AuthorisedPerson authorisedPerson =
                        new AuthorisedPerson(businessProfile, inputShareholder.Name, AuthorisationLevel.Assistant, inputShareholder.DateOfBirth,
                                                nationality, idCode, inputShareholder.IDNumber, inputShareholder.IDExpiryDate, inputShareholder.ResidentialAddress, inputShareholder.ZipCodePostCode, countryOfResidence,
                                                inputShareholder.PositionTitle, inputShareholder.EmailAddress, titleCode, inputShareholder.TitleOthers);

                        authorisedPerson.Shareholder = individual;
                        individual.AuthorisedPerson = authorisedPerson;

                        await _businessProfileRepository.AddAuthorisedPersonAsync(authorisedPerson);
                        await _businessProfileRepository.UpdateShareholderAsync(individual, cancellationToken);

                    }
                    //for new/existing shareholder if toggle is turned on
                    if (!hasOriUBO && inputShareholder.isUltimateBeneficialOwners)
                    {
                        IndividualLegalEntity ultimateBeneficialOwner =
                            new IndividualLegalEntity(businessProfile, inputShareholder.Name, inputShareholder.EffectiveShareholding, inputShareholder.ResidentialAddress, inputShareholder.ZipCodePostCode, idCode, inputShareholder.IDNumber,
                                                        nationality, gender, inputShareholder.DateOfBirth, inputShareholder.IDExpiryDate, countryOfResidence, inputShareholder.PositionTitle,
                                                        titleCode, inputShareholder.TitleOthers, null, null);

                        ultimateBeneficialOwner.Shareholder = individual;
                        individual.UltimateBeneficialOwner = ultimateBeneficialOwner;

                        await _businessProfileRepository.AddLegalEntityAsync(ultimateBeneficialOwner);
                        await _businessProfileRepository.UpdateShareholderAsync(individual, cancellationToken);
                    }



                    //for new/existing shareholder if toggle is turned off
                    if (hasOriBOD && !inputShareholder.isBoardOfDirector)
                    {
                        deletedBoardOfDirectors.Add(shareholder.BoardOfDirector);
                    }
                    if (hasOriPrimaryOfficer && !inputShareholder.isPrimaryOfficers)
                    {
                        deletedPrimaryOfficers.Add(shareholder.PrimaryOfficer);
                    }
                    if (hasOriAuthorisedPerson && !inputShareholder.isAuthorisedPerson)
                    {
                        deletedAuthorisedPerson.Add(shareholder.AuthorisedPerson);
                    }
                    if (hasOriUBO && !inputShareholder.isUltimateBeneficialOwners)
                    {
                        deletedUltimateBeneficialOwners.Add(shareholder.UltimateBeneficialOwner);
                    }
                }

                else if (inputShareholder.ShareholderTypeCode == ShareholderType.Company.Id)
                {
                    var country = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputShareholder.CountryISO2);

                    var companyName = inputShareholder.CompanyName;
                    var regNo = inputShareholder.CompanyRegNo;
                    var dateOfIncorpo = inputShareholder.DateOfIncorporation;

                    var shareholderRecords = await _businessProfileService.GetShareholderByBusinessProfileCodeAsync(businessProfile);
                    var shareholderRecord = shareholderRecords.Value.FirstOrDefault();


                    CompanyShareholder company = shareholder != null ? shareholder as CompanyShareholder : null;

                    if (company is null)
                    {
                        company = new CompanyShareholder(businessProfile, dateOfIncorpo, inputShareholder.EffectiveShareholding,
                                                         companyName, regNo, country, null, null, null);

                        company.IsQuarterlyOwned = inputShareholder.IsQuarterlyOwned;

                        var newShareholder = await _businessProfileService.AddShareholderAsync(businessProfile, company);

                        foreach (var a in inputShareholder.ShareholderLegalEntities)
                        {
                            if (inputShareholder?.IsQuarterlyOwned == true)
                            {


                                {
                                    if (a.ShareholderTypeCode == ShareholderType.Individual.Id)
                                    {
                                        var gender = await _businessProfileRepository.GetGenderTypeByCode(a.GenderCode);
                                        var idCode = await _businessProfileRepository.GetIDTypeByCode(a.IdTypeCode);
                                        var nationalityCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(a.NationalityISO2);
                                        var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(a.CountryOfResidenceISO2);
                                        var dateOfBirth = a.DateOfBirth;
                                        var titleCode = await _businessProfileRepository.GetTitleTypeByCode(a.TitleCode);

                                        ShareholderIndividualLegalEntity shareholderIndividualLegalEntity =
                                            new ShareholderIndividualLegalEntity(businessProfile, a.CompanyName,
                                                                 a.EffectiveShareholding,
                                                                 idCode, a.IDNumber, nationalityCountry, gender, dateOfBirth,
                                                                a.IDExpiryDate, countryOfResidence, newShareholder, null, titleCode, a.TitleOthers);

                                        var saveShareholderIndvidualLegal = await _businessProfileService.AddShareholderIndividualLegalEntityAsync(businessProfile, shareholderIndividualLegalEntity);

                                    }

                                    if (a.ShareholderTypeCode == ShareholderType.Company.Id)
                                    {
                                        var legalEntityCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(a.CountryISO2);
                                        ShareholderCompanyLegalEntity shareholderCompanyLegalEntity =
                                      new ShareholderCompanyLegalEntity(businessProfile, a.CompanyName, a.CompanyRegNo,
                                                            a.NameOfSharesAboveTenPercent, a.EffectiveShareholding, a.DateOfIncorporation, legalEntityCountry, newShareholder, null);

                                        var saveShareholderCompanyLegalEntity = await _businessProfileService.AddShareholderCompanyLegalEntityAsync(businessProfile, shareholderCompanyLegalEntity);


                                    }

                                }
                            }
                        }

                    }
                    else
                    {
                        if (shareholder != null)
                        {
                            company.Country = country;
                            company.DateOfIncorporation = dateOfIncorpo;
                            company.EffectiveShareholding = inputShareholder.EffectiveShareholding;
                            company.CompanyName = companyName;
                            company.CompanyRegNo = regNo;
                            company.IsQuarterlyOwned = inputShareholder.IsQuarterlyOwned;

                            var result = await _businessProfileService.UpdateShareholderAsync(businessProfile, company, cancellationToken);

                            if (inputShareholder?.IsQuarterlyOwned == true)
                            {
                                foreach (var a in inputShareholder?.ShareholderLegalEntities)
                                {
                                    var gender = await _businessProfileRepository.GetGenderTypeByCode(a.GenderCode);
                                    var idCode = await _businessProfileRepository.GetIDTypeByCode(a.IdTypeCode);
                                    var shareholderLegalEntitycountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(a.CountryISO2);
                                    var nationalityCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(a.NationalityISO2);
                                    var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(a.CountryOfResidenceISO2);
                                    var dateOfBirth = a.DateOfBirth;
                                    var shareholderLegalEntities = await _businessProfileRepository.GetLegalEntityByBusinessProfileCodeAsync(businessProfile);
                                    var titleCode = await _businessProfileRepository.GetTitleTypeByCode(a.TitleCode);

                                    foreach (var sh in shareholderRecords.Value)
                                    {

                                        var shareholderCompanyLegalEntity = await _businessProfileService.GetShareholderCompanyLegalEntity(sh.Id);
                                        var shareholderCompanyLegalEntityRecords = await _businessProfileRepository.GetShareholderCompanyLegalEntityByLegalEntityCodeAsync(a.LegalEntityCode);
                                        var shareholderIndividualLegalEntity = await _businessProfileService.GetShareholderIndividualLegalEntity(sh.Id);
                                        var shareholderIndividualLegalEntityRecords = await _businessProfileRepository.GetShareholderIndividualLegalEntityByLegalEntityCodeAsync(a.LegalEntityCode);

                                        if (result.Value.Id == sh.Id)
                                        {
                                            if (inputShareholder?.IsQuarterlyOwned == false)
                                            {
                                                List<long> deletedLegalEntityIds = shareholderCompanyLegalEntity.Select(existing => existing.Id).ToList();
                                                await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedLegalEntityIds, businessProfile.Id);
                                                foreach (var z in shareholderCompanyLegalEntity)
                                                {
                                                    await _businessProfileService.DeleteShareholderCompanyLegalEntityAsync(businessProfile, z, cancellationToken);
                                                }
                                                List<long> deletedLegalEntityIdss = shareholderIndividualLegalEntity.Select(existing => existing.Id).ToList();
                                                await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedLegalEntityIdss, businessProfile.Id);
                                                foreach (var d in shareholderIndividualLegalEntity)
                                                {
                                                    await _businessProfileService.DeleteShareholderIndividualLegalEntityAsync(businessProfile, d, cancellationToken);
                                                }

                                            }

                                            else if (shareholderCompanyLegalEntityRecords != null && inputShareholder?.IsQuarterlyOwned == true)
                                            {
                                                if (a.ShareholderTypeCode == ShareholderType.Company.Id)
                                                {
                                                    foreach (var b in shareholderCompanyLegalEntity)

                                                        if (b.Id == shareholderCompanyLegalEntityRecords.Id)
                                                        {
                                                            b.CompanyName = a.CompanyName;
                                                            b.NameOfSharesAboveTenPercent = a.NameOfSharesAboveTenPercent;
                                                            b.CompanyRegNo = a.CompanyRegNo;
                                                            b.Country = shareholderLegalEntitycountry;
                                                            b.DateOfIncorporation = a.DateOfIncorporation;
                                                            b.EffectiveShareholding = a.EffectiveShareholding;
                                                            await _businessProfileService.UpdateShareholderCompanyLegalEntityAsync(b);
                                                        }
                                                }

                                            }
                                            else if (inputShareholder.ShareholderLegalEntities.Any(x => x.ShareholderTypeCode == ShareholderType.Company.Id))
                                            {

                                                if (a.ShareholderTypeCode == ShareholderType.Company.Id)
                                                {
                                                    var legalEntityCountry = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(a.CountryISO2);
                                                    ShareholderCompanyLegalEntity newShareholderCompanyLegalEntity =
                                                      new ShareholderCompanyLegalEntity(businessProfile, a.CompanyName, a.CompanyRegNo,
                                                                           a.NameOfSharesAboveTenPercent, a.EffectiveShareholding, dateOfIncorpo, legalEntityCountry, shareholder, null);

                                                    var saveShareholderCompanyLegalEntity = await _businessProfileService.AddShareholderCompanyLegalEntityAsync(businessProfile, newShareholderCompanyLegalEntity);
                                                }
                                            }


                                            else if (shareholderIndividualLegalEntityRecords != null && shareholder.IsQuarterlyOwned == true)
                                            {
                                                if (a.ShareholderTypeCode == ShareholderType.Individual.Id)
                                                {
                                                    foreach (var c in shareholderIndividualLegalEntity)
                                                        if (c.Id == shareholderIndividualLegalEntityRecords.Id)
                                                        {
                                                            c.Name = a.CompanyName;
                                                            c.EffectiveShareholding = a.EffectiveShareholding;
                                                            c.Nationality = nationalityCountry;
                                                            c.CountryOfResidence = countryOfResidence;
                                                            c.IDType = idCode;
                                                            c.IDNumber = a.IDNumber;
                                                            c.IDExpiryDate = a.IDExpiryDate;
                                                            c.DateOfBirth = a.DateOfBirth;
                                                            c.Gender = gender;
                                                            c.Title = titleCode;
                                                            c.TitleOthers = a.TitleOthers;
                                                            await _businessProfileService.UpdateShareholderIndividualLegalEntityAsync(c);
                                                        }
                                                }

                                            }
                                            else if (inputShareholder.ShareholderLegalEntities.Any(x => x.ShareholderTypeCode == ShareholderType.Individual.Id))
                                            {
                                                if (a.ShareholderTypeCode == ShareholderType.Individual.Id)
                                                {
                                                    ShareholderIndividualLegalEntity newShareholderIndividualLegalEntity =
                                                        new ShareholderIndividualLegalEntity(businessProfile, a.CompanyName,
                                                                             a.EffectiveShareholding,
                                                                             idCode, a.IDNumber, nationalityCountry, gender, dateOfBirth,
                                                                            a.IDExpiryDate, countryOfResidence, shareholder, null, titleCode, a.TitleOthers);

                                                    var saveShareholderIndvidualLegal = await _businessProfileService.AddShareholderIndividualLegalEntityAsync(businessProfile, newShareholderIndividualLegalEntity);
                                                }
                                            }


                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                else
                {
                    return Result.Failure<IEnumerable<ShareholderOutputDTO>>(
                        $"Invalid ShareholderTypeCode[{inputShareholder.ShareholderTypeCode}]");
                }

            }


            //To Delete
            var deletedShareholder = from existing in shareholders
                                     let fromInput = request.Shareholders
                                     .FirstOrDefault(input =>
                                     input.IsDeleted &&
                                     input.ShareholderCode.HasValue &&
                                     input.ShareholderCode.Value == existing.Id)
                                     where fromInput?.ShareholderCode == existing?.Id
                                     select existing;


            if (deletedShareholder.Any())
            {
                foreach (var deletedHolder in deletedShareholder)
                {
                    if (deletedHolder.BoardOfDirector?.Id != null)
                    {
                        deletedBoardOfDirectors.Add(deletedHolder.BoardOfDirector);
                    }

                    if (deletedHolder.PrimaryOfficer?.Id != null)
                    {
                        deletedPrimaryOfficers.Add(deletedHolder.PrimaryOfficer);
                    }

                    if (deletedHolder.AuthorisedPerson?.Id != null)
                    {
                        deletedAuthorisedPerson.Add(deletedHolder.AuthorisedPerson);
                    }

                    if (deletedHolder.UltimateBeneficialOwner?.Id != null)
                    {
                        deletedUltimateBeneficialOwners.Add(deletedHolder.UltimateBeneficialOwner);
                    }
                }

                // Delete the associated screening inputs for deleted shareholders
                if (deletedShareholder.Any())
                {
                    List<long> deletedShareholderIds = deletedShareholder.Select(s => s.Id).ToList();
                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedShareholderIds, businessProfile.Id);
                }

                // Delete the associated screening inputs for deleted board of directors
                if (deletedBoardOfDirectors.Any())
                {
                    List<long> deletedBoardOfDirectorIds = deletedBoardOfDirectors.Select(b => b.Id).ToList();
                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedBoardOfDirectorIds, businessProfile.Id);
                }

                // Delete the associated screening inputs for deleted primary officers
                if (deletedPrimaryOfficers.Any())
                {
                    List<long> deletedPrimaryOfficerIds = deletedPrimaryOfficers.Select(p => p.Id).ToList();
                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedPrimaryOfficerIds, businessProfile.Id);
                }

                // Delete the associated screening inputs for deleted authorized persons
                if (deletedAuthorisedPerson.Any())
                {
                    List<long> deletedAuthorisedPersonIds = deletedAuthorisedPerson.Select(a => a.Id).ToList();
                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedAuthorisedPersonIds, businessProfile.Id);
                }
                if (deletedUltimateBeneficialOwners.Any())
                {
                    List<long> deletedUltimateBeneficialOwnerIds = deletedUltimateBeneficialOwners.Select(a => a.Id).ToList();
                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedUltimateBeneficialOwnerIds, businessProfile.Id);
                }

                //Delete the other shareholder legal entities
                foreach (var z in deletedShareholder)
                {

                    var shareholderCompanyLegalEntity = await _businessProfileService.GetShareholderCompanyLegalEntity(z.Id);
                    List<long> deletedLegalEntityIds = shareholderCompanyLegalEntity.Select(existing => existing.Id).ToList();
                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedLegalEntityIds, businessProfile.Id);
                    foreach (var d in shareholderCompanyLegalEntity)
                    {

                        await _businessProfileService.DeleteShareholderCompanyLegalEntityAsync(businessProfile, d, cancellationToken);
                    }
                }

                foreach (var d in deletedShareholder)
                {

                    var shareholderIndividualLegalEntity = await _businessProfileService.GetShareholderIndividualLegalEntity(d.Id);
                    List<long> deletedLegalEntityIds = shareholderIndividualLegalEntity.Select(existing => existing.Id).ToList();
                    await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedLegalEntityIds, businessProfile.Id);
                    foreach (var b in shareholderIndividualLegalEntity)
                    {
                        await _businessProfileService.DeleteShareholderIndividualLegalEntityAsync(businessProfile, b, cancellationToken);
                    }
                }


                // Delete the other related entities
                await _businessProfileService.DeleteShareholderAsync(businessProfile, deletedShareholder, cancellationToken);

            }

            await _businessProfileRepository.DeleteBoardOfDirectorAsync(deletedBoardOfDirectors, cancellationToken);
            await _businessProfileRepository.DeletePrimaryOfficerAsync(deletedPrimaryOfficers, cancellationToken);
            await _businessProfileRepository.DeleteAuthorisedPersonAsync(deletedAuthorisedPerson, cancellationToken);
            await _businessProfileRepository.DeleteIndividualLegalEntityAsync(deletedUltimateBeneficialOwners, cancellationToken);

            shareholdersResult = await _businessProfileService.GetShareholderByBusinessProfileCodeAsync(businessProfile);
            var _isShareholderCompleted = await _businessProfileService.IsOwnershipShareholderCompleted(businessProfile.Id);

            // Check if any Shareholders remain after updates or deletions
            var remainingShareholders = await _businessProfileService.GetShareholderByBusinessProfileCodeAsync(businessProfile);

            // After updates or deletions
            if (remainingShareholders.IsSuccess && !remainingShareholders.Value.Any())
            {
                // All Shareholder are deleted, set the concurrency token to null
                businessProfile.ShareholderConcurrencyToken = null;
            }
            else
            {
                // Update the concurrency token since some  Shareholder are still present
                businessProfile.ShareholderConcurrencyToken = Guid.NewGuid();
            }

            // Update the business profile
            await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

            var outputDTO = new List<ShareholderOutputDTO>();

            foreach (var s in shareholdersResult.Value)
            {
                var individualLegalEntitiesOutput = new List<ShareholderIndividualLegalEntityOutputDTO>();

                var individualLegalEntities = await _businessProfileService.GetShareholderIndividualLegalEntity(s.Id);

                if (individualLegalEntities != null)
                {
                    individualLegalEntitiesOutput = _mapper.Map<IEnumerable<ShareholderIndividualLegalEntity>, List<ShareholderIndividualLegalEntityOutputDTO>>(individualLegalEntities);
                }

                var companyLegalEntitiesOutput = new List<ShareholderCompanyLegalEntityOutputDTO>();

                var companyLegalEntities = await _businessProfileService.GetShareholderCompanyLegalEntity(s.Id);

                if (companyLegalEntities != null)
                {
                    companyLegalEntitiesOutput = _mapper.Map<IEnumerable<ShareholderCompanyLegalEntity>, List<ShareholderCompanyLegalEntityOutputDTO>>(companyLegalEntities);
                }

                outputDTO.Add(new ShareholderOutputDTO()
                {
                    DateOfIncorporation = s.DateOfIncorporation,
                    EffectiveShareholding = s.EffectiveShareholding,
                    ShareholderCode = s.Id,
                    Name = s is IndividualShareholder individualShareholder ? individualShareholder.Name : null,
                    DateOfBirth = s is IndividualShareholder individualShareholderDOB ? individualShareholderDOB.DateOfBirth : null,
                    IdTypeCode = s is IndividualShareholder individualShareholderIDType ? individualShareholderIDType.IDType?.Id : null,
                    IDNumber = s is IndividualShareholder individualShareholderIDNumber ? individualShareholderIDNumber.IDNumber : null,
                    IDExpiryDate = s is IndividualShareholder individualShareholderIDExpiry ? individualShareholderIDExpiry.IDExpiryDate : null,
                    GenderCode = s is IndividualShareholder individualShareholderGender ? individualShareholderGender.Gender?.Id : null,
                    NationalityISO2 = s is IndividualShareholder individualShareholderNationality ? individualShareholderNationality.Nationality?.CountryISO2 : null,
                    CompanyName = s is CompanyShareholder companyShareholder ? companyShareholder.CompanyName : null,
                    CompanyRegNo = s is CompanyShareholder companyShareHolderRegNo ? companyShareHolderRegNo.CompanyRegNo : null,
                    CountryISO2 = s is CompanyShareholder companyShareholderCountry ? companyShareholderCountry.Country?.CountryISO2 : null,
                    CountryOfResidenceISO2 = s is IndividualShareholder individualShareholderCountryOfResidence ? individualShareholderCountryOfResidence.CountryOfResidence?.CountryISO2 : null,
                    ShareholderTypeCode = s is IndividualShareholder ? 1 : 2,
                    IsCompleted = _isShareholderCompleted,
                    AuthorisedPersonCode = s.AuthorisedPerson?.Id,
                    PrimaryOfficersCode = s.PrimaryOfficer?.Id,
                    BoardOfDirectorCode = s.BoardOfDirector?.Id,
                    ResidentialAddress = s is IndividualShareholder individualShareholderResidentialAddress ? individualShareholderResidentialAddress.ResidentialAddress : null,
                    ZipCodePostCode = s is IndividualShareholder individualShareholderZipCodePostCode ? individualShareholderZipCodePostCode.ZipCodePostCode : null,
                    PositionTitle = s is IndividualShareholder positionTitle ? positionTitle.PositionTitle : null,
                    ShareholderConcurrencyToken = businessProfile.ShareholderConcurrencyToken,
                    ShareholderIndividualLegalEntityOutputDTOs = individualLegalEntitiesOutput,
                    ShareholderCompanyLegalEntityOutputDTOs = companyLegalEntitiesOutput,
                });
            }

            return Result.Success(outputDTO as IEnumerable<ShareholderOutputDTO>);

        }

        private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.ShareholderConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.ShareholderConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<ShareholderOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.ShareholderConcurrencyToken = Guid.NewGuid();
                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<ShareholderOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
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
                _logger.LogError(ex, "[{0}]", nameof(SaveShareholderCommandHandler));
            }
        }
    }
}
