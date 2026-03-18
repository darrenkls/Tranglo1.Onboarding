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
using Tranglo1.Onboarding.Application.DTO.BoardofDirector;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
    [Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    internal class SaveBoardOfDirectorCommand : BaseCommand<Result<IEnumerable<BoardofDirectorOutputDTO>>>
    {
        public IEnumerable<BoardofDirectorInputDTO> BoardOfDirectors;
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public Guid? BoardOfDirectorConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<IEnumerable<BoardofDirectorOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Board of Directors for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveBoardOfDirectorCommandHandler : IRequestHandler<SaveBoardOfDirectorCommand, Result<IEnumerable<BoardofDirectorOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveBoardOfDirectorCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IConfiguration _config;

        public SaveBoardOfDirectorCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                ILogger<SaveBoardOfDirectorCommandHandler> logger,
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

        public async Task<Result<IEnumerable<BoardofDirectorOutputDTO>>> Handle(SaveBoardOfDirectorCommand request, CancellationToken cancellationToken)
        {
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);


            if (businessProfileList.IsFailure)
            {
                return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>("Invalid Business Profile");
            }
            var businessProfile = businessProfileList.Value;
            Result<IEnumerable<BoardofDirectorOutputDTO>> result = new Result<IEnumerable<BoardofDirectorOutputDTO>>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);

            // Handle concurrency
            var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
            {
                var concurrencyCheck = ConcurrencyCheck(request.BoardOfDirectorConcurrencyToken, businessProfile);
                if (concurrencyCheck.Result.IsFailure)
                {
                    return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(concurrencyCheck.Result.Error);
                }
            }

            if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
            {
                if (applicationUser is CustomerUser && businessProfile.BusinessKYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete ||
                kycBusinessReviewResult != null))
                {
                    //update
                    result = await UpdateBoardOfDirector(request, businessProfile, cancellationToken);

                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(
                                            $"Customer user is unable to update for {request.BusinessProfileCode}."
                                            );
                    }

                    await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode,
                        KYCCategory.Business_Ownership.Id,
                        cancellationToken);
                }

                else if (applicationUser is TrangloStaff &&
                    ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                    businessProfile.BusinessKYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                    || kycBusinessReviewResult != null)))
                {
                    //update
                    result = await UpdateBoardOfDirector(request, businessProfile, cancellationToken);

                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(
                                            $"Admin user is unable to update for {request.BusinessProfileCode}."
                                            );
                    }

                    //check mandatory fields
                    //temporary commenting due to design issue on API causing deadlock
                    //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
                }

                else
                {
                    return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(
                                             $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                             );
                }
            }
            else if (request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id)
            {
                if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete ||
                 kycBusinessReviewResult != null))
                {
                    //update
                    result = await UpdateBoardOfDirector(request, businessProfile, cancellationToken);

                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(
                                            $"Customer user is unable to update for {request.BusinessProfileCode}."
                                            );
                    }

                }

                else if (applicationUser is TrangloStaff &&
                    ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                    businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                    || kycBusinessReviewResult != null)))
                {
                    //update
                    result = await UpdateBoardOfDirector(request, businessProfile, cancellationToken);

                    if (result.IsFailure)
                    {
                        return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(
                                            $"Admin user is unable to update for {request.BusinessProfileCode}."
                                            );
                    }

                    //check mandatory fields
                    //temporary commenting due to design issue on API causing deadlock
                    //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
                }

                else
                {
                    return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(
                                             $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                             );
                }
            }



            return result;
        }

        private async Task<Result<IEnumerable<BoardofDirectorOutputDTO>>> UpdateBoardOfDirector(SaveBoardOfDirectorCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {


            var BOD = await _businessProfileService.GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile);

            IReadOnlyList<BoardOfDirector> boardOfDirectors =
                BOD.IsSuccess ? BOD.Value : Enumerable.Empty<BoardOfDirector>().ToList().AsReadOnly();

            //To Add

            foreach (var inputBOD in request.BoardOfDirectors)
            {


                if (inputBOD.BoardOfDirectorCode == 0 || inputBOD.BoardOfDirectorCode == null)
                {

                    var nationality = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputBOD.NationalityISO2);
                    var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputBOD.CountryOfResidenceISO2);
                    var titleCode = await _businessProfileRepository.GetTitleTypeByCode(inputBOD.TitleCode);

                    var gender = await _businessProfileRepository.GetGenderTypeByCode(inputBOD.GenderCode);
                    var idCode = await _businessProfileRepository.GetIDTypeByCode(inputBOD.IdTypeCode);
                    var dateOfBirth = inputBOD.DateOfBirth;
                    var fullname = inputBOD.Name;



                    BoardOfDirector boardOfDirector =
                        new BoardOfDirector(businessProfile, inputBOD.Name, inputBOD.PositionTitle,
                                            dateOfBirth, gender, idCode, inputBOD.IDNumber, inputBOD.IDExpiryDate, nationality,
                                            countryOfResidence, inputBOD.ResidentialAddress, inputBOD.ZipCodePostCode, titleCode, inputBOD.TitleOthers);

                    await _businessProfileService.AddBoardOfDirectorAsync(businessProfile, boardOfDirector);


                }

                //TO Update
                else if (!inputBOD.IsDeleted)
                {
                    var _ExistingBoardOfDirector = boardOfDirectors
                        .Where(x => x.Id == inputBOD.BoardOfDirectorCode)
                        .FirstOrDefault();

                    if (_ExistingBoardOfDirector == null)
                    {
                        return Result.Failure<IEnumerable<BoardofDirectorOutputDTO>>(
                            $"Invalid BoardOfDirectorCode [{inputBOD.BoardOfDirectorCode}].");

                    }

                    var nationality = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputBOD.NationalityISO2);
                    var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputBOD.CountryOfResidenceISO2);
                    var gender = await _businessProfileRepository.GetGenderTypeByCode(inputBOD.GenderCode);
                    var idCode = await _businessProfileRepository.GetIDTypeByCode(inputBOD.IdTypeCode);
                    var titleCode = await _businessProfileRepository.GetTitleTypeByCode(inputBOD.TitleCode);
                    var dateOfBirth = inputBOD.DateOfBirth;



                    _ExistingBoardOfDirector.Nationality = nationality;
                    _ExistingBoardOfDirector.IDType = idCode;
                    _ExistingBoardOfDirector.Gender = gender;
                    _ExistingBoardOfDirector.CountryOfResidence = countryOfResidence;


                    _ExistingBoardOfDirector.Name = inputBOD.Name;
                    _ExistingBoardOfDirector.PositionTitle = inputBOD.PositionTitle;
                    _ExistingBoardOfDirector.DateOfBirth = inputBOD.DateOfBirth;
                    _ExistingBoardOfDirector.IDNumber = inputBOD.IDNumber;
                    _ExistingBoardOfDirector.IDExpiryDate = inputBOD.IDExpiryDate;
                    _ExistingBoardOfDirector.ResidentialAddress = inputBOD.ResidentialAddress;
                    _ExistingBoardOfDirector.ZipCodePostCode = inputBOD.ZipCodePostCode;
                    _ExistingBoardOfDirector.Title = titleCode;
                    _ExistingBoardOfDirector.TitleOthers = inputBOD.TitleOthers;




                    await _businessProfileService.UpdateBoardOfDirectorAsync(businessProfile, _ExistingBoardOfDirector, cancellationToken);
                }
            }

            //To Delete

            var deletedBoardOfDirectors = from existing in boardOfDirectors
                                          let fromInput = request.BoardOfDirectors
                                          .FirstOrDefault(input =>
                                          input.IsDeleted &&
                                          input.BoardOfDirectorCode.HasValue &&
                                          input.BoardOfDirectorCode.Value == existing.Id
                                          )
                                          where fromInput?.BoardOfDirectorCode == existing?.Id
                                          select existing;

            if (deletedBoardOfDirectors.Any())
            {
                List<long> deletedBoardOfDirectorIds = deletedBoardOfDirectors.Select(existing => existing.Id).ToList();

                await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedBoardOfDirectorIds, businessProfile.Id);
                await _businessProfileService.DeleteBoardOfDirectorAsync(businessProfile, deletedBoardOfDirectors, cancellationToken);
            }

            BOD = await _businessProfileService.GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile);
            var _isBoardOfDirectorCompleted = await _businessProfileService.IsOwnershipBoardOfDirectorCompleted(businessProfile.Id);

            // Check if any Board of Directors remain after updates or deletions
            var remainingBoardOfDirectors = await _businessProfileService.GetBoardOfDirectorByBusinessProfileCodeAsync(businessProfile);

            // After updates or deletions
            if (remainingBoardOfDirectors.IsSuccess && !remainingBoardOfDirectors.Value.Any())
            {
                // All Board of Directors are deleted, set the concurrency token to null
                businessProfile.BoardOfDirectorConcurrencyToken = null;
            }
            else
            {
                // Update the concurrency token since some Board of Directors are still present
                businessProfile.BoardOfDirectorConcurrencyToken = Guid.NewGuid();
            }

            // Update the business profile
            await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

            return Result.Success<IEnumerable<BoardofDirectorOutputDTO>>(BOD.Value.Select(s => new BoardofDirectorOutputDTO()
            {
                Name = s.Name,
                PositionTitle = s.PositionTitle,
                DateOfBirth = s.DateOfBirth,
                GenderCode = s.Gender?.Id,
                IdTypeCode = s.IDType?.Id,
                IDNumber = s.IDNumber,
                IDExpiryDate = s.IDExpiryDate,
                BoardOfDirectorCode = s.Id,
                NationalityISO2 = s.Nationality?.CountryISO2,
                CountryOfResidenceISO2 = s.CountryOfResidence?.CountryISO2,
                ResidentialAddress = s.ResidentialAddress,
                ZipCodePostCode = s.ZipCodePostCode,
                isCompleted = _isBoardOfDirectorCompleted,
                ShareholderCode = s.Shareholder?.Id,
                BoardOfDirectorConcurrencyToken = s.BusinessProfile.BoardOfDirectorConcurrencyToken,
                TitleCode = s.Title?.Id,
                TitleOthers = s.TitleOthers
            }));
        }

        private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.BoardOfDirectorConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.BoardOfDirectorConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<BoardofDirectorOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.BoardOfDirectorConcurrencyToken = Guid.NewGuid();
                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<BoardofDirectorOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
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
                _logger.LogError(ex, "[{0}]", nameof(SaveBoardOfDirectorCommandHandler));
            }
        }
    }
}
