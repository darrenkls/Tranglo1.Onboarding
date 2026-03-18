using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common.SingleScreening;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.ScreeningAggregate;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Requests;
using Tranglo1.Onboarding.Domain.ExternalServices.Compliance.Models.Responses;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO.Watchlist;

namespace Tranglo1.Onboarding.Domain.DomainServices
{
    public class ComplianceScreeningService
    {
        private readonly IComplianceExternalService _complianceExternalService;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public ComplianceScreeningService(IComplianceExternalService complianceExternalService, 
            IBusinessProfileRepository businessProfileRepository)
        {
            _complianceExternalService = complianceExternalService;
            _businessProfileRepository = businessProfileRepository;
        }

        /// <summary>
        /// Return list of ChangeDTO if there is any changes detected during screening
        /// </summary>
        /// <param name="businessProfileCode"></param>
        /// <returns>ChangeDTO used for sending email notification</returns>
        public async Task<Result<ComplianceScreeningOutputDTO>> ScreeningAsync(int businessProfileCode)
        {
            var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(businessProfileCode);
            if (businessProfile == null)
            {
                return Result.Failure<ComplianceScreeningOutputDTO>($"Business Profile with code {businessProfileCode} not found.");
            }

            // Upsert Screening Inputs
            var latestScreeningInputs = await UpsertScreeningInputsAsync(businessProfile);

            // Result to return
            var result = new ComplianceScreeningOutputDTO()
            {
                ScreeningInputCount = latestScreeningInputs.Count
            };

            // Get previous screenings for comparison
            var latestScreeningInputIds = latestScreeningInputs.Select(x => x.Id).ToHashSet().ToList();
            var previousScreenings = await _businessProfileRepository.GetLatestScreeningByScreeningInputCodes(latestScreeningInputIds);

            // Perform screening for each screening input
            foreach (var latestScreeningInput in latestScreeningInputs)
            {
                // Skip calling external API when mandatory FullName is missing to avoid unnecessary failures
                if (string.IsNullOrEmpty(latestScreeningInput.FullName))
                {
                    continue;
                }

                var nameScreenerRequest = new NameScreenerRequest(
                    entityType: latestScreeningInput.GetScreeningEntityType(),
                    fullName: latestScreeningInput.FullName,
                    dateOfBirth: latestScreeningInput.DateOfBirth,
                    nationality: latestScreeningInput.GetNationalityMeta()?.CountryISO2,
                    gender: latestScreeningInput.Gender);

                var nameScreenerResult = await _complianceExternalService.ScreeningAsync(nameScreenerRequest);
                if (nameScreenerResult.IsFailure)
                {
                    // Full Name is a mandatory field in Screening API, the API will return BadRequest if Full Name is null or empty string
                    // Hence, if any failure occurs here meaning Full Name is NOT null or empty string, we should log the failed screening input for further investigation
                    result.FailedNameScreeningInputs.Add(new FailedScreeningInput
                    {
                        OwnershipStructureTypeId = latestScreeningInput.OwnershipStrucureTypeId.Value,
                        TableId = latestScreeningInput.TableId
                    });
                    continue;
                }

                // SIDE TOPIC: Collect Single Screening Result Output DTOs for RSA dependency in SubmitBusinessUserKYCCommand
                var summary = nameScreenerResult.Value.Summary;
                result.SingleScreeningListResultOutputDTOs.Add(new SingleScreeningListResultOutputDTO
                {
                    Reference = nameScreenerResult.Value.Reference,
                    Summary = new Summary()
                    {
                        SanctionList = summary.SanctionList,
                        PEP = summary.PEP,
                        SOE = summary.SOE,
                        AdverseMedia = summary.AdverseMedia,
                        Enforcement = summary.Enforcement,
                        AssociatedEntity = summary.AssociatedEntity
                    }
                });

                // Call Get Matched Entity API if there is any compliance hit
                IReadOnlyList<GetEntityDetailByReferenceCodeResponse> getEntityDetailResponses = new List<GetEntityDetailByReferenceCodeResponse>();
                if (nameScreenerResult.Value.Summary.HasComplianceHit())
                {
                    var getEntityDetailRequest = await _complianceExternalService.GetEntityDetailsByReferenceCodeAsync(nameScreenerResult.Value.Reference);
                    if (getEntityDetailRequest.IsFailure)
                    {
                        // Log the failed screening input for further investigation
                        result.FailedGetMatchedEntityScreeningInputs.Add(new FailedScreeningInput
                        {
                            OwnershipStructureTypeId = latestScreeningInput.OwnershipStrucureTypeId.Value,
                            TableId = latestScreeningInput.TableId
                        });
                        continue;
                    }
                    getEntityDetailResponses = getEntityDetailRequest.Value;
                }

                // Update or Insert Screening Result
                latestScreeningInput.UpsertScreeningResult(nameScreenerResult.Value, getEntityDetailResponses);

                // Get ChangeDTOs for email notification by comparing with previous screening
                var previousScreening = previousScreenings.FirstOrDefault(s => s.ScreeningInputCode == latestScreeningInput.Id);

                var currentChangeDto = GetChangeDTO(businessProfile, latestScreeningInput, previousScreening);
                if (currentChangeDto != null)
                {
                    result.ChangeDTOs.Add(currentChangeDto);
                }

                // Update Watchlist Status to pending review if there is any new entry category detected
                if (currentChangeDto != null)
                {
                    latestScreeningInput.UpdateWatchlistStatus(WatchlistStatus.PendingReview);
                }
            }

            // Commit Screening Results to Database (impacting ScreeningInputs, Screenings and ScreeningDetail tables)
            await _businessProfileRepository.SaveChangesAsync();

            return Result.Success(result);
        }

        #region Private Helper Methods
        private async Task<List<ScreeningInput>> UpsertScreeningInputsAsync(BusinessProfile businessProfile)
        {
            // Get Existing Screening Inputs (to be updated)
            var existingScreeningInputs = await _businessProfileRepository.GetScreeningInputsByBusinessProfileIdAsync(businessProfile.Id);

            // New Screening Input to be created
            var newScreeningInputs = new List<ScreeningInput>();

            UpsertBusinessProfileScreeningInput(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertCompanyShareholderScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertIndividualShareholderScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertIndividualLegalEntityScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertBoardOfDirectorScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertPrimaryOfficerScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertAffiliateScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertAuthorizedPersonScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertShareholderIndividualLegalEntityScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);
            await UpsertShareholderCompanyLegalEntityScreeningInputsAsync(businessProfile, existingScreeningInputs, newScreeningInputs);

            // Update existing screening input and Save new screening input
            await _businessProfileRepository.UpdateScreeningInputAsync(existingScreeningInputs.ToList());
            await _businessProfileRepository.AddScreeningInputAsync(newScreeningInputs);

            // return latest screening input
            return existingScreeningInputs.Concat(newScreeningInputs).ToList();
        }

        private static void UpsertBusinessProfileScreeningInput(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var existingScreeningInput = existingScreeningInputs
                .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.BusinessProfile) && si.TableId == businessProfile.Id);

            if (existingScreeningInput != null)
            {
                // Update existing screening input
                existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Natural.Id;
                existingScreeningInput.FullName = businessProfile.CompanyRegistrationName;
                existingScreeningInput.DateOfBirth = businessProfile.DateOfIncorporation;
                existingScreeningInput.NationalityCountryCode = businessProfile.CompanyRegisteredCountryMeta?.Id;
                existingScreeningInput.Gender = null;
            }
            else
            {
                // Create new Screening Input
                var newScreeningInput = new ScreeningInput(
                    screeningEntityType: ScreeningEntityType.Natural,
                    businessProfile: businessProfile,
                    ownershipStrucureType: OwnershipStrucureType.BusinessProfile,
                    tableId: businessProfile.Id,
                    fullName: businessProfile.CompanyRegistrationName,
                    dateOfBirth: businessProfile.DateOfIncorporation,
                    nationalityMeta: businessProfile.CompanyRegisteredCountryMeta,
                    gender: null);

                newScreeningInputs.Add(newScreeningInput);
            }
        }

        private async Task UpsertCompanyShareholderScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var companyShareholders = await _businessProfileRepository.GetCompanyShareholderByBusinessProfileCodeAsync(businessProfile);
            foreach (var companyShareholder in companyShareholders)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.CompanyShareholder) && si.TableId == companyShareholder.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Natural.Id;
                    existingScreeningInput.FullName = companyShareholder.CompanyName;
                    existingScreeningInput.DateOfBirth = companyShareholder.DateOfIncorporation;
                    existingScreeningInput.NationalityCountryCode = companyShareholder.Country?.Id;
                    existingScreeningInput.Gender = null;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Natural,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.CompanyShareholder,
                        tableId: companyShareholder.Id,
                        fullName: companyShareholder.CompanyName,
                        dateOfBirth: companyShareholder.DateOfIncorporation,
                        nationalityMeta: companyShareholder.Country,
                        gender: null);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertIndividualShareholderScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var individualShareholders = await _businessProfileRepository.GetIndividualShareholderByBusinessProfileCodeAsync(businessProfile);
            foreach (var individualShareholder in individualShareholders)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.IndividualShareholder) && si.TableId == individualShareholder.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Individual.Id;
                    existingScreeningInput.FullName = individualShareholder.Name;
                    existingScreeningInput.DateOfBirth = individualShareholder.DateOfBirth;
                    existingScreeningInput.NationalityCountryCode = individualShareholder.Nationality?.Id;
                    existingScreeningInput.Gender = individualShareholder.Gender?.Name;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Individual,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.IndividualShareholder,
                        tableId: individualShareholder.Id,
                        fullName: individualShareholder.Name,
                        dateOfBirth: individualShareholder.DateOfBirth,
                        nationalityMeta: individualShareholder.Nationality,
                        gender: individualShareholder.Gender?.Name);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertIndividualLegalEntityScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var individualLegalEntities = await _businessProfileRepository.GetIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile);
            foreach (var individualLegalEntity in individualLegalEntities)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.IndividualLegalEntity) && si.TableId == individualLegalEntity.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Individual.Id;
                    existingScreeningInput.FullName = individualLegalEntity.CompanyName;
                    existingScreeningInput.DateOfBirth = individualLegalEntity.DateOfBirth;
                    existingScreeningInput.NationalityCountryCode = individualLegalEntity.Nationality?.Id;
                    existingScreeningInput.Gender = individualLegalEntity.Gender?.Name;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Individual,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.IndividualLegalEntity,
                        tableId: individualLegalEntity.Id,
                        fullName: individualLegalEntity.CompanyName,
                        dateOfBirth: individualLegalEntity.DateOfBirth,
                        nationalityMeta: individualLegalEntity.Nationality,
                        gender: individualLegalEntity.Gender?.Name);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertBoardOfDirectorScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var boardOfDirectors = await _businessProfileRepository.GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile);
            foreach (var boardOfDirector in boardOfDirectors)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.BoardOfDirector) && si.TableId == boardOfDirector.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Individual.Id;
                    existingScreeningInput.FullName = boardOfDirector.Name;
                    existingScreeningInput.DateOfBirth = boardOfDirector.DateOfBirth;
                    existingScreeningInput.NationalityCountryCode = boardOfDirector.Nationality?.Id;
                    existingScreeningInput.Gender = boardOfDirector.Gender?.Name;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Individual,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.BoardOfDirector,
                        tableId: boardOfDirector.Id,
                        fullName: boardOfDirector.Name,
                        dateOfBirth: boardOfDirector.DateOfBirth,
                        nationalityMeta: boardOfDirector.Nationality,
                        gender: boardOfDirector.Gender?.Name);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertPrimaryOfficerScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var primaryOfficers = await _businessProfileRepository.GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile);
            foreach (var primaryOfficer in primaryOfficers)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.PrimaryOfficer) && si.TableId == primaryOfficer.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Individual.Id;
                    existingScreeningInput.FullName = primaryOfficer.Name;
                    existingScreeningInput.DateOfBirth = primaryOfficer.DateOfBirth;
                    existingScreeningInput.NationalityCountryCode = primaryOfficer.Nationality?.Id;
                    existingScreeningInput.Gender = primaryOfficer.Gender?.Name;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Individual,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.PrimaryOfficer,
                        tableId: primaryOfficer.Id,
                        fullName: primaryOfficer.Name,
                        dateOfBirth: primaryOfficer.DateOfBirth,
                        nationalityMeta: primaryOfficer.Nationality,
                        gender: primaryOfficer.Gender?.Name);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertAffiliateScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var affiliates = await _businessProfileRepository.GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile);
            foreach (var affiliate in affiliates)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.AffiliatesAndSubsidiaries) && si.TableId == affiliate.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Natural.Id;
                    existingScreeningInput.FullName = affiliate.CompanyName;
                    existingScreeningInput.DateOfBirth = affiliate.DateOfIncorporation;
                    existingScreeningInput.NationalityCountryCode = affiliate.Country?.Id;
                    existingScreeningInput.Gender = null;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Natural,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.AffiliatesAndSubsidiaries,
                        tableId: affiliate.Id,
                        fullName: affiliate.CompanyName,
                        dateOfBirth: affiliate.DateOfIncorporation,
                        nationalityMeta: affiliate.Country,
                        gender: null);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertAuthorizedPersonScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var authorizedPersons = await _businessProfileRepository.GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile);
            foreach (var authorizedPerson in authorizedPersons)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.AuthorisedPerson) && si.TableId == authorizedPerson.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Individual.Id;
                    existingScreeningInput.FullName = authorizedPerson.FullName;
                    existingScreeningInput.DateOfBirth = authorizedPerson.DateOfBirth;
                    existingScreeningInput.NationalityCountryCode = authorizedPerson.Nationality?.Id;
                    existingScreeningInput.Gender = null; // ZK TODO: Futher CR required to add Gender to AuthorizedPerson
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Individual,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.AuthorisedPerson,
                        tableId: authorizedPerson.Id,
                        fullName: authorizedPerson.FullName,
                        dateOfBirth: authorizedPerson.DateOfBirth,
                        nationalityMeta: authorizedPerson.Nationality,
                        gender: null); // ZK TODO: Futher CR required to add Gender to AuthorizedPerson

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertShareholderIndividualLegalEntityScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var shareholderIndividualLegalEntities = await _businessProfileRepository.GetShareholderIndividualLegalEntityByBusinessProfileCodeAsync(businessProfile);
            foreach (var shareholderIndividualLegalEntity in shareholderIndividualLegalEntities)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.ShareholderIndividualLegalEntity) && si.TableId == shareholderIndividualLegalEntity.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening input
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Individual.Id;
                    existingScreeningInput.FullName = shareholderIndividualLegalEntity.Name;
                    existingScreeningInput.DateOfBirth = shareholderIndividualLegalEntity.DateOfBirth;
                    existingScreeningInput.NationalityCountryCode = shareholderIndividualLegalEntity.Nationality?.Id;
                    existingScreeningInput.Gender = shareholderIndividualLegalEntity.Gender?.Name;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Individual,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.ShareholderIndividualLegalEntity,
                        tableId: shareholderIndividualLegalEntity.Id,
                        fullName: shareholderIndividualLegalEntity.Name,
                        dateOfBirth: shareholderIndividualLegalEntity.DateOfBirth,
                        nationalityMeta: shareholderIndividualLegalEntity.Nationality,
                        gender: shareholderIndividualLegalEntity.Gender?.Name);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private async Task UpsertShareholderCompanyLegalEntityScreeningInputsAsync(BusinessProfile businessProfile, IEnumerable<ScreeningInput> existingScreeningInputs, List<ScreeningInput> newScreeningInputs)
        {
            var shareholderCompanyLegalEntities = await _businessProfileRepository.GetShareholderCompanyLegalEntityByBusinessProfileCodeAsync(businessProfile);
            foreach (var shareholderCompanyLegalEntity in shareholderCompanyLegalEntities)
            {
                var existingScreeningInput = existingScreeningInputs
                    .FirstOrDefault(si => si.OwnershipStrucureType.Equals(OwnershipStrucureType.ShareholderCompanyLegalEntity) && si.TableId == shareholderCompanyLegalEntity.Id);

                if (existingScreeningInput != null)
                {
                    // Update existing screening inputs
                    existingScreeningInput.ScreeningEntityTypeId = ScreeningEntityType.Natural.Id;
                    existingScreeningInput.FullName = shareholderCompanyLegalEntity.CompanyName;
                    existingScreeningInput.DateOfBirth = shareholderCompanyLegalEntity.DateOfIncorporation;
                    existingScreeningInput.NationalityCountryCode = shareholderCompanyLegalEntity.Country?.Id;
                    existingScreeningInput.Gender = null;
                }
                else
                {
                    // Create new Screening Input
                    var newScreeningInput = new ScreeningInput(
                        screeningEntityType: ScreeningEntityType.Natural,
                        businessProfile: businessProfile,
                        ownershipStrucureType: OwnershipStrucureType.ShareholderCompanyLegalEntity,
                        tableId: shareholderCompanyLegalEntity.Id,
                        fullName: shareholderCompanyLegalEntity.CompanyName,
                        dateOfBirth: shareholderCompanyLegalEntity.DateOfIncorporation,
                        nationalityMeta: shareholderCompanyLegalEntity.Country,
                        gender: null);

                    newScreeningInputs.Add(newScreeningInput);
                }
            }
        }

        private static ChangeDTO GetChangeDTO(BusinessProfile businessProfile, ScreeningInput latestScreeningInput, Screening previousScreening)
        {
            // When the screening details in the latest screening contains values, it indicates there are compliance hits
            var latestScreening = latestScreeningInput.Screenings.OrderByDescending(s => s.ScreeningDate).FirstOrDefault();

            // If there are no screening details in the latest screening, return empty list
            if (latestScreening?.ScreeningDetails == null || latestScreening.ScreeningDetails.Count == 0)
            {
                return null;
            }

            // If there's no previous screening, all details are new changes
            if (previousScreening?.ScreeningDetails == null || previousScreening.ScreeningDetails.Count == 0)
            {
                var screeningDetailCategories = latestScreening.ScreeningDetails
                    .Where(x => x.ScreeningDetailsCategoryId.HasValue)
                    .Select(x => x.GetScreeningDetailsCategory()?.Name)
                    .Distinct();

                return new ChangeDTO(
                    solutions: businessProfile.GetSolutions(),
                    companyName: businessProfile.CompanyName,
                    ownershipStrucureType: latestScreeningInput.GetOwnershipStrucureType(),
                    screeningEntityType: latestScreeningInput.GetScreeningEntityType(),
                    fullName: latestScreeningInput.FullName,
                    nationality: latestScreeningInput.GetNationalityMeta(),
                    dateOfBirth: latestScreeningInput.DateOfBirth,
                    screeningDetailCategories: screeningDetailCategories,
                    entityIds: latestScreening.ScreeningDetails.Select(x => x.EntityId));
            }

            // Compare latest screening details with previous screening details
            var latestDetails = latestScreening.ScreeningDetails
                .Select(d => (d.EntityId, d.ListingDate, d.ScreeningListSourceCode))
                .ToHashSet();

            var previousDetails = previousScreening.ScreeningDetails
                .Select(d => (d.EntityId, d.ListingDate, d.ScreeningListSourceCode))
                .ToHashSet();

            if (latestDetails.SetEquals(previousDetails))
            {
                // No changes detected
                return null;
            }

            // Find new screening details entries
            var newDetails = latestDetails.Except(previousDetails).ToHashSet();

            var newDetailCategoryEntryies = latestScreening.ScreeningDetails
                    .Where(x => newDetails.Contains((x.EntityId, x.ListingDate, x.ScreeningListSourceCode)) && x.ScreeningDetailsCategoryId.HasValue)
                    .Select(x => x.GetScreeningDetailsCategory()?.Name)
                    .Distinct();

            return new ChangeDTO(
                    solutions: businessProfile.GetSolutions(),
                    companyName: businessProfile.CompanyRegistrationName,
                    ownershipStrucureType: latestScreeningInput.GetOwnershipStrucureType(),
                    screeningEntityType: latestScreeningInput.GetScreeningEntityType(),
                    fullName: latestScreeningInput.FullName,
                    nationality: latestScreeningInput.GetNationalityMeta(),
                    dateOfBirth: latestScreeningInput.DateOfBirth,
                    screeningDetailCategories: newDetailCategoryEntryies,
                    entityIds: latestScreening.ScreeningDetails.Select(x => x.EntityId));
        }
        #endregion Private Helper Methods
    }
}
