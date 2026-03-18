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
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.PrimaryOfficer;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
    [Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    internal class SavePrimaryOfficerCommand : BaseCommand<Result<IEnumerable<PrimaryOfficerOutputDTO>>>
    {
        public IEnumerable<PrimaryOfficerInputDTO> PrimaryOfficers;
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public Guid? PrimaryOfficerConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<IEnumerable<PrimaryOfficerOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Parent Holding Companies for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SavePrimaryOfficerCommandHandler : IRequestHandler<SavePrimaryOfficerCommand, Result<IEnumerable<PrimaryOfficerOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SavePrimaryOfficerCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IConfiguration _config;

        public SavePrimaryOfficerCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                ILogger<SavePrimaryOfficerCommandHandler> logger,
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

        public async Task<Result<IEnumerable<PrimaryOfficerOutputDTO>>> Handle(SavePrimaryOfficerCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            if (businessProfileList.IsFailure)
            {
                return Result.Failure<IEnumerable<PrimaryOfficerOutputDTO>>("Invalid Business Profile");
            }
            var businessProfile = businessProfileList.Value;
            Result<IEnumerable<PrimaryOfficerOutputDTO>> result = new Result<IEnumerable<PrimaryOfficerOutputDTO>>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);

            // Handle concurrency
            var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
            {
                var concurrencyCheck = ConcurrencyCheck(request.PrimaryOfficerConcurrencyToken, businessProfile);
                if (concurrencyCheck.Result.IsFailure)
                {
                    return Result.Failure<IEnumerable<PrimaryOfficerOutputDTO>>(concurrencyCheck.Result.Error);
                }
            }

            if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete
                || kycBusinessReviewResult == ReviewResult.Insufficient_Incomplete))
            {
                //update
                result = await UpdatePrimaryOfficer(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<PrimaryOfficerOutputDTO>>(
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
                result = await UpdatePrimaryOfficer(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<PrimaryOfficerOutputDTO>>(
                                        $"Admin user is unable to update for {request.BusinessProfileCode}."
                                        );
                }

                //check mandatory fields
                //temporary commenting due to design issue on API causing deadlock
                //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);

            }

            else
            {
                return Result.Failure<IEnumerable<PrimaryOfficerOutputDTO>>(
                                         $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                         );
            }

            return result;
        }

        private async Task<Result<IEnumerable<PrimaryOfficerOutputDTO>>> UpdatePrimaryOfficer(SavePrimaryOfficerCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {


            var primaryOfficer = await _businessProfileService.GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile);

            IReadOnlyList<PrimaryOfficer> primaryOfficers =
                primaryOfficer.IsSuccess ? primaryOfficer.Value : Enumerable.Empty<PrimaryOfficer>().ToList().AsReadOnly();

            //To Add
            foreach (var inputPrimaryOfficer in request.PrimaryOfficers)
            {

                if (inputPrimaryOfficer.PrimaryOfficerCode == 0 || inputPrimaryOfficer.PrimaryOfficerCode == null)
                {

                    var nationality = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputPrimaryOfficer.NationalityISO2);
                    var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputPrimaryOfficer.CountryOfResidenceISO2);
                    var gender = await _businessProfileRepository.GetGenderTypeByCode(inputPrimaryOfficer.GenderCode);
                    var idCode = await _businessProfileRepository.GetIDTypeByCode(inputPrimaryOfficer.IdTypeCode);
                    var titleCode = await _businessProfileRepository.GetTitleTypeByCode(inputPrimaryOfficer.TitleCode);

                    PrimaryOfficer primary =
                        new PrimaryOfficer(businessProfile, inputPrimaryOfficer.Name, inputPrimaryOfficer.PositionTitle,
                                            inputPrimaryOfficer.DateOfBirth, gender, idCode,
                                                inputPrimaryOfficer.IDNumber, inputPrimaryOfficer.IDExpiryDate, nationality, countryOfResidence,
                                                inputPrimaryOfficer.ResidentialAddress, inputPrimaryOfficer.ZipCodePostCode, titleCode, inputPrimaryOfficer.TitleOthers);

                    await _businessProfileService.AddPrimaryOfficerAsync(businessProfile, primary);

                }

                //To Update
                else
                {
                    var _ExistingPrimaryOfficer = primaryOfficers
                        .Where(x => x.Id == inputPrimaryOfficer.PrimaryOfficerCode)
                        .FirstOrDefault();

                    if (_ExistingPrimaryOfficer == null)
                    {
                        return Result.Failure<IEnumerable<PrimaryOfficerOutputDTO>>(
                            $"Invalid Primary Officer Code[{inputPrimaryOfficer.PrimaryOfficerCode}].");
                    }
                    var nationality = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputPrimaryOfficer.NationalityISO2);
                    var countryOfResidence = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputPrimaryOfficer.CountryOfResidenceISO2);
                    var gender = await _businessProfileRepository.GetGenderTypeByCode(inputPrimaryOfficer.GenderCode);
                    var idCode = await _businessProfileRepository.GetIDTypeByCode(inputPrimaryOfficer.IdTypeCode);
                    var titleCode = await _businessProfileRepository.GetTitleTypeByCode(inputPrimaryOfficer.TitleCode);

                    if (inputPrimaryOfficer.TitleOthers != null && inputPrimaryOfficer.TitleOthers.Length > 50)
                    {
                        return Result.Failure<IEnumerable<PrimaryOfficerOutputDTO>>(
                                             $"Title cannot more than 50 characters. "
                                             );
                    }

                    _ExistingPrimaryOfficer.Nationality = nationality;
                    _ExistingPrimaryOfficer.IDType = idCode;
                    _ExistingPrimaryOfficer.Gender = gender;
                    _ExistingPrimaryOfficer.CountryOfResidence = countryOfResidence;

                    _ExistingPrimaryOfficer.Name = inputPrimaryOfficer.Name;
                    _ExistingPrimaryOfficer.PositionTitle = inputPrimaryOfficer.PositionTitle;
                    _ExistingPrimaryOfficer.DateOfBirth = inputPrimaryOfficer.DateOfBirth;
                    _ExistingPrimaryOfficer.IDNumber = inputPrimaryOfficer.IDNumber;
                    _ExistingPrimaryOfficer.IDExpiryDate = inputPrimaryOfficer.IDExpiryDate;
                    _ExistingPrimaryOfficer.ResidentialAddress = inputPrimaryOfficer.ResidentialAddress;
                    _ExistingPrimaryOfficer.ZipCodePostCode = inputPrimaryOfficer.ZipCodePostCode;
                    _ExistingPrimaryOfficer.Title = titleCode;
                    _ExistingPrimaryOfficer.TitleOthers = inputPrimaryOfficer.TitleOthers;

                    await _businessProfileService.UpdatePrimaryOfficerAsync(businessProfile, _ExistingPrimaryOfficer, cancellationToken);

                }
            }

            //TO Delete
            var deletedPrimaryOfficers = from existing in primaryOfficers
                                         let fromInput = request.PrimaryOfficers
                                         .FirstOrDefault(input =>
                                         input.IsDeleted &&
                                         input.PrimaryOfficerCode.HasValue &&
                                         input.PrimaryOfficerCode.Value ==
                                         existing.Id)
                                         where fromInput?.PrimaryOfficerCode == existing?.Id
                                         select existing;

            if (deletedPrimaryOfficers.Any())
            {
                List<long> deletedPrimaryOfficersIds = deletedPrimaryOfficers.Select(existing => existing.Id).ToList();

                await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedPrimaryOfficersIds, businessProfile.Id);
                await _businessProfileService.DeletePrimaryOfficerAsync(deletedPrimaryOfficers, cancellationToken);
            }

            primaryOfficer = await _businessProfileService.GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile);
            var _isPrimaryOfficerCompleted = await _businessProfileService.IsOwnershipPrimaryOfficerCompleted(businessProfile.Id);

            // Check if any Primary Officer remain after updates or deletions
            var remainingPrimaryOfficers = await _businessProfileService.GetPrimaryOfficerByBusinessProfileCodeAsync(businessProfile);

            // After updates or deletions
            if (remainingPrimaryOfficers.IsSuccess && !remainingPrimaryOfficers.Value.Any())
            {
                // All Primary Officers are deleted, set the concurrency token to null
                businessProfile.PrimaryOfficerConcurrencyToken = null;
            }
            else
            {
                // Update the concurrency token since some  Primary Officers are still present
                businessProfile.PrimaryOfficerConcurrencyToken = Guid.NewGuid();
            }

            // Update the business profile
            await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

            return Result.Success<IEnumerable<PrimaryOfficerOutputDTO>>(primaryOfficer.Value.Select(s => new PrimaryOfficerOutputDTO()
            {
                Name = s.Name,
                PositionTitle = s.PositionTitle,
                DateOfBirth = s.DateOfBirth,
                GenderCode = s.Gender?.Id,
                IdTypeCode = s.IDType?.Id,
                IDNumber = s.IDNumber,
                IDExpiryDate = s.IDExpiryDate,
                PrimaryOfficerCode = s.Id,
                NationalityISO2 = s.Nationality?.CountryISO2,
                CountryOfResidenceISO2 = s.CountryOfResidence?.CountryISO2,
                ResidentialAddress = s.ResidentialAddress,
                ZipCodePostCode = s.ZipCodePostCode,
                isCompleted = _isPrimaryOfficerCompleted,
                ShareholderCode = s.Shareholder?.Id,
                PrimaryOfficerConcurrencyToken = businessProfile.PrimaryOfficerConcurrencyToken,
                TitleCode = s.Title?.Id,
                TitleOthers = s.TitleOthers
            }));
        }

        private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.PrimaryOfficerConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.PrimaryOfficerConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<PrimaryOfficerOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.PrimaryOfficerConcurrencyToken = Guid.NewGuid();
                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<PrimaryOfficerOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
            }
        }
    }
}
