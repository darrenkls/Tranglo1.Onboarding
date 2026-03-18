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
using Tranglo1.Onboarding.Application.DTO.OwnershipAndManagementStructure.AuthorisedPerson;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    [Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    internal class SaveAuthorisedPersonCommand : BaseCommand<Result<IEnumerable<AuthorisedPersonOutputDTO>>>
    {
        public IEnumerable<AuthorisedPersonInputDTO> AuthorisedPeople;
        public int BusinessProfileCode { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public Guid? AuthorisedPersonConcurrencyToken { get; set; }


        public override Task<string> GetAuditLogAsync(Result<IEnumerable<AuthorisedPersonOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Authorised Person for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveAuthorisedPersonCommandHandler : IRequestHandler<SaveAuthorisedPersonCommand, Result<IEnumerable<AuthorisedPersonOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveAuthorisedPersonCommandHandler> _logger;
        private readonly IBusinessProfileRepository _repository;

        private readonly IApplicationUserRepository _aplicationUserRepository;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IExternalUserRoleRepository _externalUserRoleRepository;
        private readonly IMediator _mediator;
        private readonly IConfiguration _config;



        public SaveAuthorisedPersonCommandHandler(
            BusinessProfileService businessProfileService,
            ILogger<SaveAuthorisedPersonCommandHandler> logger,
            IBusinessProfileRepository repository,

            IApplicationUserRepository applicationUserRepository,
            TrangloUserManager userManager,
             PartnerService partnerService,
             IScreeningRepository screeningRepository,
             IExternalUserRoleRepository externalUserRoleRepository,
             IMediator mediator, IConfiguration config)
        {
            this._businessProfileService = businessProfileService;
            this._logger = logger;
            this._repository = repository;

            _aplicationUserRepository = applicationUserRepository;
            this._userManager = userManager;
            this._partnerService = partnerService;
            this._screeningRepository = screeningRepository;
            this._externalUserRoleRepository = externalUserRoleRepository;
            this._mediator = mediator;
            this._config = config;
        }

        public async Task<Result<IEnumerable<AuthorisedPersonOutputDTO>>> Handle(SaveAuthorisedPersonCommand request, CancellationToken cancellationToken)
        {
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var isMainAuthorisedPerson = request.AuthorisedPeople.Any(x => x.AuthorisationLevelCode == 1);

            //To Not Allow Main Authorised Person to be deleted
            //if (isMainAuthorisedPerson == true && request.AuthorisedPeople.Any(x => x.isDeleted == true))
            //{
            //    return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>("Main Authorised Person cannot be deleted");
            //}

            if (businessProfileList.IsFailure)
            {
                return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>("Invalid Business Profile");
            }

            var businessProfile = businessProfileList.Value;
            Result<IEnumerable<AuthorisedPersonOutputDTO>> result = new Result<IEnumerable<AuthorisedPersonOutputDTO>>();
            var kycReviewResult = await _repository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);
            var kycBusinessReviewResult = await _repository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);


            // Handle concurrency
            var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
            {
                var concurrencyCheck = ConcurrencyCheck(request.AuthorisedPersonConcurrencyToken, businessProfile);
                if (concurrencyCheck.Result.IsFailure)
                {
                    return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>(concurrencyCheck.Result.Error);
                }
            }

            //Customer
            if (ClaimCode.Connect == request.CustomerSolution)
            {

                if (applicationUser is CustomerUser && (businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft || businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted) && (kycReviewResult == ReviewResult.Insufficient_Incomplete
                    || kycBusinessReviewResult == ReviewResult.Insufficient_Incomplete))
                {
                    //update
                    result = await UpdateAuthorisedPerson(request, businessProfile, applicationUser, cancellationToken);

                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Customer user is unable to update for {request.BusinessProfileCode}.");
                    }

                }
            }
            else if (ClaimCode.Business == request.CustomerSolution)
            {
                //update
                result = await UpdateAuthorisedPerson(request, businessProfile, applicationUser, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Customer user is unable to update for {request.BusinessProfileCode}.");
                }

                await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode,
                    KYCCategory.Business_Ownership.Id,
                    cancellationToken);
            }
            //Tranglo Staff
            else if (Solution.Connect.Id == request.AdminSolution)
            {
                if (applicationUser is TrangloStaff &&
                ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                || kycBusinessReviewResult != null)))
                {
                    //update
                    result = await UpdateAuthorisedPerson(request, businessProfile, applicationUser, cancellationToken);

                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Admin user is unable to update for {request.BusinessProfileCode}.");
                    }

                    //check mandatory fields
                    //temporary commenting due to design issue on API causing deadlock
                    //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
                }
            }

            else if (Solution.Business.Id == request.AdminSolution)
            {
                //update
                result = await UpdateAuthorisedPerson(request, businessProfile, applicationUser, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Admin user is unable to update for {request.BusinessProfileCode}.");
                }
            }
            else
            {
                return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Unable to update for BusinessProfileCode {request.BusinessProfileCode}.");
            }

            return result;
        }

        private async Task<Result<IEnumerable<AuthorisedPersonOutputDTO>>> UpdateAuthorisedPerson(SaveAuthorisedPersonCommand request, BusinessProfile businessProfile, ApplicationUser requestUser, CancellationToken cancellationToken)
        {
            var authorisedPeople = await _businessProfileService.GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile);


            IReadOnlyList<AuthorisedPerson> authorisedPersons =
           authorisedPeople.IsSuccess ? authorisedPeople.Value : Enumerable.Empty<AuthorisedPerson>().ToList().AsReadOnly();


            // Remove AuthorisationLevel on this ticket TBT-82 6h
            ////Assign the next available authorized person as the new MAIN authorized person.
            //if (businessProfile.SolutionCode == Solution.Business.Id)
            //{
            //    bool hasMain = request.AuthorisedPeople.Where(x => x.AuthorisationLevelCode == 1 && !x.isDeleted).Any();
            //    bool hasRecord = request.AuthorisedPeople.Where(x => !x.isDeleted).Any();
            //
            //    if (!hasMain && hasRecord)
            //    {
            //        AuthorisedPersonInputDTO mainAuthorisedPeople = request.AuthorisedPeople.Where(x => !x.isDeleted).Last();
            //        mainAuthorisedPeople.AuthorisationLevelCode = 1;
            //    }
            //}

            //To Add
            foreach (var inputAuthorisedPerson in request.AuthorisedPeople)
            {
                Result<ContactNumber> createTelephoneNumber = string.IsNullOrWhiteSpace(inputAuthorisedPerson.TelephoneNumber) ? null : ContactNumber.Create(inputAuthorisedPerson.TelephoneDialCode, inputAuthorisedPerson.TelephoneNumberCountryISO2, inputAuthorisedPerson.TelephoneNumber);
                if (createTelephoneNumber.IsFailure)
                {
                    return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Create contact number failed for {inputAuthorisedPerson.TelephoneNumber}. {createTelephoneNumber.Error}");
                }

                if (inputAuthorisedPerson.AuthorisedPersonCode == 0 || inputAuthorisedPerson.AuthorisedPersonCode == null)
                {
                    var nationality = await _repository.GetCountryMetaByISO2CodeAsync(inputAuthorisedPerson.NationalityISO2);
                    var countryOfResidence = await _repository.GetCountryMetaByISO2CodeAsync(inputAuthorisedPerson.CountryOfResidenceISO2);

                    var idCode = await _repository.GetIDTypeByCode(inputAuthorisedPerson.IdTypeCode);
                    //var authorisationLevelCode = await _repository.GetAuthorisationLevelCodeByCode(inputAuthorisedPerson.AuthorisationLevelCode);
                    var dateOfBirth = inputAuthorisedPerson.DateOfBirth;
                    var titleCode = await _repository.GetTitleTypeByCode(inputAuthorisedPerson.TitleCode);

                    AuthorisedPerson authorisedPerson =
                        new AuthorisedPerson(businessProfile, inputAuthorisedPerson.FullName, null, dateOfBirth, nationality,
                        idCode ?? null, inputAuthorisedPerson.IDNumber, inputAuthorisedPerson.IDExpiryDate, inputAuthorisedPerson.ResidentialAddress,
                        inputAuthorisedPerson.ZipCodePostCode, countryOfResidence, inputAuthorisedPerson.PositionTitle, inputAuthorisedPerson.EmailAddress, titleCode, inputAuthorisedPerson.TitleOthers, false);

                    authorisedPerson.TelephoneNumber = createTelephoneNumber.Value;

                    await _businessProfileService.AddAuthorisedPersonAsync(businessProfile, authorisedPerson);
                }

                //To Update
                else if (!inputAuthorisedPerson.isDeleted)
                {
                    var _ExistingAuthorisedPerson = authorisedPersons
                       .FirstOrDefault(x => x.Id == inputAuthorisedPerson.AuthorisedPersonCode);

                    if (_ExistingAuthorisedPerson == null)
                    {
                        return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Invalid BoardOfDirectorCode [{inputAuthorisedPerson.AuthorisedPersonCode}].");
                    }

                    var nationality = await _repository.GetCountryMetaByISO2CodeAsync(inputAuthorisedPerson.NationalityISO2);
                    var countryOfResidence = await _repository.GetCountryMetaByISO2CodeAsync(inputAuthorisedPerson.CountryOfResidenceISO2);
                    var idCode = await _repository.GetIDTypeByCode(inputAuthorisedPerson.IdTypeCode);
                    var dateOfBirth = inputAuthorisedPerson.DateOfBirth;
                    var titleCode = await _repository.GetTitleTypeByCode(inputAuthorisedPerson.TitleCode);
                    //var authorisationLevelCode = await _repository.GetAuthorisationLevelCodeByCode(inputAuthorisedPerson.AuthorisationLevelCode);
                    _ExistingAuthorisedPerson.CountryOfResidence = countryOfResidence;
                    _ExistingAuthorisedPerson.IDType = idCode;
                    _ExistingAuthorisedPerson.DateOfBirth = dateOfBirth;
                    _ExistingAuthorisedPerson.Nationality = nationality;
                    _ExistingAuthorisedPerson.IDNumber = inputAuthorisedPerson.IDNumber;
                    _ExistingAuthorisedPerson.IDExpiryDate = inputAuthorisedPerson.IDExpiryDate;
                    _ExistingAuthorisedPerson.ResidentialAddress = inputAuthorisedPerson.ResidentialAddress;
                    _ExistingAuthorisedPerson.ZipCodePostCode = inputAuthorisedPerson.ZipCodePostCode;
                    _ExistingAuthorisedPerson.FullName = inputAuthorisedPerson.FullName;
                    _ExistingAuthorisedPerson.PositionTitle = inputAuthorisedPerson.PositionTitle;
                    _ExistingAuthorisedPerson.TelephoneNumber = createTelephoneNumber.Value;
                    _ExistingAuthorisedPerson.EmailAddress = inputAuthorisedPerson.EmailAddress;
                    _ExistingAuthorisedPerson.Title = titleCode;
                    _ExistingAuthorisedPerson.TitleOthers = inputAuthorisedPerson.TitleOthers;
                    //if( !_ExistingAuthorisedPerson.IsDefault)
                    //{
                    //    _ExistingAuthorisedPerson.FullName = inputAuthorisedPerson.FullName;
                    //}

                    //AuthorisationLevel should be disabled and not allowed to edit, so there is no need to be updated
                    //_ExistingAuthorisedPerson.AuthorisationLevel = authorisationLevelCode;

                    await _businessProfileService.UpdateAuthorisedPerson(businessProfile, _ExistingAuthorisedPerson, cancellationToken);
                }
            }

            //To Delete 
            var deletedAuthorisedPerson = from existing in authorisedPersons
                                          let fromInput = request.AuthorisedPeople
                                          .FirstOrDefault(input =>
                                          input.isDeleted &&
                                          !input.IsDefault && //Only allow to delete non default record
                                          input.AuthorisedPersonCode.HasValue &&
                                          input.AuthorisedPersonCode.Value == existing.Id
                                          )
                                          where fromInput?.AuthorisedPersonCode == existing?.Id
                                          select existing;

            if (deletedAuthorisedPerson.Any())
            {
                List<long> deletedAuthorisedPersonIds = deletedAuthorisedPerson.Select(existing => existing.Id).ToList();

                await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedAuthorisedPersonIds, businessProfile.Id);
                await _businessProfileService.DeleteAuthorisedPersonAsync(deletedAuthorisedPerson, cancellationToken);

                foreach (var authorisedPerson in deletedAuthorisedPerson)
                {
                    // Get Authorised Person UserId by Email Address, cannot use BusinessProileID/RequesterID to find CustomerUserBusinessProfile because requester can be Admin
                    var applicationUser = await _aplicationUserRepository.GetApplicationUserByLoginId(authorisedPerson.EmailAddress);

                    if (applicationUser != null)
                    {
                        // Remove Checker Role for Deleted Authorised Person for TB only
                        CustomerUserBusinessProfile customerUserBusinessProfile = await _repository.GetCustomerUserBusinessProfilesByUserIdAsync(applicationUser.Id, businessProfile.Id);
                        var roleList = await _externalUserRoleRepository.GetAllExternalUserRolesBySolution(Solution.Business.Id);
                        var checkerRoleCode = roleList.FirstOrDefault(x => x.ExternalUserRoleName.ToLower() == "checker")?.RoleCode;

                        if (!string.IsNullOrEmpty(checkerRoleCode) && customerUserBusinessProfile != null)
                        {
                            CustomerUserBusinessProfileRole customerUserBusinessProfileRole = await _repository.GetCustomerUserBusinessProfileRolesByCodeAsync(customerUserBusinessProfile?.Id, checkerRoleCode);

                            var resultDeleteCUBPR = await _repository.DeleteCustomerUserBusinessProfileRoleAsync(customerUserBusinessProfileRole);

                            if (resultDeleteCUBPR.IsFailure)
                            {
                                _logger.LogError($"[SaveAuthorisedPersonCommand]: Failed to delete Authorised Person {businessProfile.CompanyName}, {customerUserBusinessProfileRole.UserRole.Name}. Error saving UserRoleCode: {checkerRoleCode}. Error: {resultDeleteCUBPR.Error}");
                                return Result.Failure<IEnumerable<AuthorisedPersonOutputDTO>>($"Failed to delete {businessProfile.CompanyName}, {customerUserBusinessProfileRole.UserRole.Name}. Error saving UserRoleCode: {checkerRoleCode}");
                            }
                        }
                    }
                }
            }

            authorisedPeople = await _businessProfileService.GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile);
            var _isAuthorisedPersonCompleted = await _businessProfileService.IsOwnershipAuthorisedPersonCompleted(businessProfile.Id);

            // Check if any authorised persons remain after updates or deletions
            var remainingAuthorizedPersons = await _businessProfileService.GetAuthorisedPersonByBusinessProfileCodeAsync(businessProfile);

            // After updates or deletions
            if (remainingAuthorizedPersons.IsSuccess && !remainingAuthorizedPersons.Value.Any())
            {
                // All authorised persons are deleted, set the concurrency token to null
                businessProfile.AuthorisedPersonConcurrencyToken = null;
            }
            else
            {
                // Update the concurrency token since some authorised persons are still present
                businessProfile.AuthorisedPersonConcurrencyToken = Guid.NewGuid();
            }

            // Update the business profile
            await _repository.UpdateBusinessProfileAsync(businessProfile);

            // Continue with the regular response
            return Result.Success<IEnumerable<AuthorisedPersonOutputDTO>>(authorisedPeople.Value.Select(s => new AuthorisedPersonOutputDTO()
            {
                AuthorisedPersonCode = s.Id,
                FullName = s.FullName,
                AuthorisationLevelCode = s.AuthorisationLevel?.Id ?? 0,
                AuthorisationLevelDescription = s.AuthorisationLevel?.Name ?? null,
                DateOfBirth = s.DateOfBirth,
                NationalityISO2 = s.Nationality?.CountryISO2,
                IdTypeCode = s.IDType?.Id ?? null,
                IdTypeDescription = s.IDType?.Name ?? null,
                IDNumber = s.IDNumber,
                IDExpiryDate = s.IDExpiryDate,
                ResidentialAddress = s.ResidentialAddress,
                ZipCodePostCode = s.ZipCodePostCode,
                CountryOfResidenceISO2 = s.CountryOfResidence?.CountryISO2,
                BusinessProfileCode = s.BusinessProfile.Id,
                ShareholderCode = s.Shareholder?.Id,
                isCompleted = _isAuthorisedPersonCompleted,
                PositionTitle = s.PositionTitle,
                TelephoneDialCode = s.TelephoneNumber != null ? s.TelephoneNumber.DialCode : null,
                TelephoneNumber = s.TelephoneNumber != null ? s.TelephoneNumber.Value : null,
                TelephoneNumberCountryISO2 = s.TelephoneNumber != null ? s.TelephoneNumber.CountryISO2Code : null,
                AuthorisedPersonConcurrencyToken = businessProfile.AuthorisedPersonConcurrencyToken,
                EmailAddress = s.EmailAddress,
                TitleCode = s.Title?.Id,
                TitleOthers = s.TitleOthers
            }));

        }

        private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.AuthorisedPersonConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.AuthorisedPersonConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<AuthorisedPersonOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.AuthorisedPersonConcurrencyToken = Guid.NewGuid();
                await _repository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<AuthorisedPersonOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
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

                await _repository.UpdateKYCSummaryFeedbackNotificationsAsReadByCategoryAsync(specification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{0}]", nameof(SaveAuthorisedPersonCommandHandler));
            }
        }
    }
}
