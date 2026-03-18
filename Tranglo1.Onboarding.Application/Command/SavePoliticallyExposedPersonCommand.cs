using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
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
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.PoliticallyExposedPerson;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
    [Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
    internal class SavePoliticallyExposedPersonCommand : BaseCommand<Result<IEnumerable<PoliticallyExposedPersonOutputDTO>>>
    {
        public IEnumerable<PoliticallyExposedPersonInputDTO> PoliticallyExposedPersons;

        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public Guid? PoliticalExposedPersonsConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<IEnumerable<PoliticallyExposedPersonOutputDTO>> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Politically Exposed Persons (PEP) for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SavePoliticallyExposedPersonCommandHandler : IRequestHandler<SavePoliticallyExposedPersonCommand, Result<IEnumerable<PoliticallyExposedPersonOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SavePoliticallyExposedPersonCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public SavePoliticallyExposedPersonCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                ILogger<SavePoliticallyExposedPersonCommandHandler> logger,
                IMapper mapper,
                PartnerService partnerService,
                IBusinessProfileRepository businessProfileRepository
            )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<Result<IEnumerable<PoliticallyExposedPersonOutputDTO>>> Handle(SavePoliticallyExposedPersonCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            if (businessProfileList.IsFailure)
            {
                return Result.Failure<IEnumerable<PoliticallyExposedPersonOutputDTO>>("Invalid Business Profile");
            }
            var businessProfile = businessProfileList.Value;
            Result<IEnumerable<PoliticallyExposedPersonOutputDTO>> result = new Result<IEnumerable<PoliticallyExposedPersonOutputDTO>>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);


            // Handle concurrency
            if (request.CustomerSolution == ClaimCode.Business || request.AdminSolution == Solution.Business.Id)
            {
                var concurrencyCheck = ConcurrencyCheck(request.PoliticalExposedPersonsConcurrencyToken, businessProfile);
                if (concurrencyCheck.Result.IsFailure)
                {
                    return Result.Failure<IEnumerable<PoliticallyExposedPersonOutputDTO>>(concurrencyCheck.Result.Error);
                }
            }

            if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && kycReviewResult == ReviewResult.Insufficient_Incomplete)
            {
                //update
                result = await UpdatePoliticallyExposedPerson(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<PoliticallyExposedPersonOutputDTO>>(
                                        $"Customer user is unable to update for {request.BusinessProfileCode}."
                                        );
                }

            }

            else if (applicationUser is TrangloStaff &&
                ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) || 
                businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || kycReviewResult == ReviewResult.Complete))
            {
                //update
                result = await UpdatePoliticallyExposedPerson(request, businessProfile, cancellationToken);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<PoliticallyExposedPersonOutputDTO>>(
                                        $"Admin user is unable to update for {request.BusinessProfileCode}."
                                        );
                }

                //check mandatory fields
                //temporary commenting due to design issue on API causing deadlock
                //await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
            }

            else
            {
                return Result.Failure<IEnumerable<PoliticallyExposedPersonOutputDTO>>(
                                         $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                         );
            }

            return result;
        }
        private async Task<Result<IEnumerable<PoliticallyExposedPersonOutputDTO>>> UpdatePoliticallyExposedPerson(SavePoliticallyExposedPersonCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {

            var politicalExposedPerson = await _businessProfileService.GetPoliticallyExposedPersonByBusinessProfileCodeAsync(businessProfile);

            IReadOnlyList<PoliticallyExposedPerson> politicallyExposedPeople =
                politicalExposedPerson.IsSuccess ? politicalExposedPerson.Value : Enumerable.Empty<PoliticallyExposedPerson>().ToList().AsReadOnly();

            //To Add Values
            foreach (var inputPEP in request.PoliticallyExposedPersons)
            {


                if (inputPEP.PoliticallyExposedPersonCode == 0 || inputPEP.PoliticallyExposedPersonCode == null)
                {

                    var nationality = CountryMeta.GetCountryByISO2Async(inputPEP.NationalityISO2);
                    var countryOfResidence = CountryMeta.GetCountryByISO2Async(inputPEP.CountryOfResidenceISO2);
                    var gender = Enumeration.FindById<Gender>(inputPEP.GenderCode.GetValueOrDefault());
                    var idCode = Enumeration.FindById<IDType>(inputPEP.IdTypeCode.GetValueOrDefault());



                    PoliticallyExposedPerson politicallyExposedPerson =
                        new PoliticallyExposedPerson(businessProfile, inputPEP.Name, inputPEP.PositionTitle,
                                                     inputPEP.DateOfBirth, gender,
                                                     idCode, inputPEP.IDNumber, inputPEP.IDExpiryDate, nationality,countryOfResidence);

                    await _businessProfileService.AddPoliticallyExposedPersonAsync(businessProfile, politicallyExposedPerson);
                }

                //To Update
                else if (!inputPEP.IsDeleted)
                {
                    var _ExistingPoliticallyExposedPerson = politicallyExposedPeople
                        .Where(x => x.Id == inputPEP.PoliticallyExposedPersonCode)
                        .FirstOrDefault();

                    if (_ExistingPoliticallyExposedPerson == null)
                    {
                        return Result.Failure<IEnumerable<PoliticallyExposedPersonOutputDTO>>(
                            $"Invalid Politically Exposed Person Code [{inputPEP.PoliticallyExposedPersonCode}].");
                    }

                    var nationality = CountryMeta.GetCountryByISO2Async(inputPEP.NationalityISO2);
                    var countryOfResidence = CountryMeta.GetCountryByISO2Async(inputPEP.CountryOfResidenceISO2);
                    var gender = Enumeration.FindById<Gender>(inputPEP.GenderCode.GetValueOrDefault());
                    var idCode = Enumeration.FindById<IDType>(inputPEP.IdTypeCode.GetValueOrDefault());



                    _ExistingPoliticallyExposedPerson.Nationality = nationality;
                    _ExistingPoliticallyExposedPerson.IDType = idCode;
                    _ExistingPoliticallyExposedPerson.Gender = gender;
                    _ExistingPoliticallyExposedPerson.CountryOfResidence = countryOfResidence;

                    _ExistingPoliticallyExposedPerson.Name = inputPEP.Name;
                    _ExistingPoliticallyExposedPerson.PositionTitle = inputPEP.PositionTitle;
                    _ExistingPoliticallyExposedPerson.DateOfBirth = inputPEP.DateOfBirth;
                    _ExistingPoliticallyExposedPerson.IDNumber = inputPEP.IDNumber;
                    _ExistingPoliticallyExposedPerson.IDExpiryDate = inputPEP.IDExpiryDate;


                    await _businessProfileService.UpdatePoliticallyExposedPersonAsync(businessProfile, _ExistingPoliticallyExposedPerson, cancellationToken);

                }
            }

            //To Delete
            var deletedPoliticallyExposedPeople = from existing in politicallyExposedPeople
                                                  let fromInput = request.PoliticallyExposedPersons
                                                  .FirstOrDefault(input =>
                                                  input.IsDeleted &&
                                                  input.PoliticallyExposedPersonCode.HasValue &&
                                                  input.PoliticallyExposedPersonCode.Value == existing.Id)
                                                  where fromInput?.PoliticallyExposedPersonCode == existing?.Id
                                                  select existing;

            if (deletedPoliticallyExposedPeople.Any())
            {
                await _businessProfileService.DeletePoliticallyExposedPersonAsync(businessProfile, deletedPoliticallyExposedPeople, cancellationToken);

            }

            politicalExposedPerson = await _businessProfileService.GetPoliticallyExposedPersonByBusinessProfileCodeAsync(businessProfile);

            // Check if any PEP remain after updates or deletions
            var remainingPoliticalExposedPersons = await _businessProfileService.GetPoliticallyExposedPersonByBusinessProfileCodeAsync(businessProfile);

            // After updates or deletions
            if (remainingPoliticalExposedPersons.IsSuccess && !remainingPoliticalExposedPersons.Value.Any())
            {
                // All PEP are deleted, set the concurrency token to null
                businessProfile.PoliticalExposedPersonsConcurrencyToken = null;
            }
            else
            {
                // Update the concurrency token since some  PEP are still present
                businessProfile.PoliticalExposedPersonsConcurrencyToken = Guid.NewGuid();
            }

            // Update the business profile
            await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

            return Result.Success<IEnumerable<PoliticallyExposedPersonOutputDTO>>(politicalExposedPerson.Value.Select(s => new PoliticallyExposedPersonOutputDTO()
            {
                Name = s.Name,
                PositionTitle = s.PositionTitle,
                DateOfBirth = s.DateOfBirth,
                GenderCode = s.Gender?.Id,
                IdTypeCode = s.IDType?.Id,
                IDNumber = s.IDNumber,
                IDExpiryDate = s.IDExpiryDate,
                PoliticallyExposedPersonCode = s.Id,
                NationalityISO2 = s.Nationality?.CountryISO2,
                CountryOfResidenceISO2 = s.CountryOfResidence?.CountryISO2,
                PoliticalExposedPersonsConcurrencyToken = businessProfile.PoliticalExposedPersonsConcurrencyToken


            }));
        }

        private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
        {
            try
            {
                if ((concurrencyToken.HasValue && businessProfile.PoliticalExposedPersonsConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.PoliticalExposedPersonsConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<PoliticallyExposedPersonOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                businessProfile.PoliticalExposedPersonsConcurrencyToken = Guid.NewGuid();
                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure<PoliticallyExposedPersonOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
            }
        }
    }
}
