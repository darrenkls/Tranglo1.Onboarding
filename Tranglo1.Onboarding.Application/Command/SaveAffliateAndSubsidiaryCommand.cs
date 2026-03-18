
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
using Tranglo1.Onboarding.Application.DTO.AffiliateAndSubsidiary;
using Tranglo1.Onboarding.Application.Models;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
	//[Permission(PermissionGroupCode.KYCOwnershipAndManagementStructure, UACAction.Edit)]
	[Permission(Permission.KYCManagementOwnership.Action_Edit_Code,
		new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
		new string[] { Permission.KYCManagementOwnership.Action_View_Code })]
	internal class SaveAffliateAndSubsidiaryCommand : BaseCommand<Result<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>>
    {
        public IEnumerable<AffiliateAndSubsidiaryInputDTO> AffiliateAndSubsidiaries;
		public int BusinessProfileCode { get; set; }
		public string CustomerSolution { get; set; }
		public long? AdminSolution { get; set; }
		public string LoginId { get; set; }
		public Guid? AffiliatesAndSubsidiariesConcurrencyToken { get; set; }


		public override Task<string> GetAuditLogAsync(Result<IEnumerable<AffiliateAndSubsidiaryOutputDTO>> result)
		{
			if(result.IsSuccess)
            {
				string _description = $"Add Affiliate and Subsidiaries for Business Profile Code: [{this.BusinessProfileCode}]";
				return Task.FromResult(_description);
			}

			return Task.FromResult<string>(null);
		}
	}

	internal class SaveAffliateAndSubsidiaryCommandHandler : IRequestHandler<SaveAffliateAndSubsidiaryCommand, Result<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveAffliateAndSubsidiaryCommandHandler> _logger;
        private readonly IMapper _mapper;
		private readonly TrangloUserManager _userManager;
		private readonly PartnerService _partnerService;
		private readonly IBusinessProfileRepository _businessProfileRepository;
		private readonly IScreeningRepository _screeningRepository;
		private readonly IConfiguration _config;

		public SaveAffliateAndSubsidiaryCommandHandler(
                TrangloUserManager userManager,
                BusinessProfileService businessProfileService,
                ILogger<SaveAffliateAndSubsidiaryCommandHandler> logger,
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

		public async Task<Result<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>> Handle(SaveAffliateAndSubsidiaryCommand request, CancellationToken cancellationToken)
		{
			var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
			var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
			var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);

			ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

			if (businessProfileList.IsFailure)
			{
				return Result.Failure<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>("Invalid business profile.");
			}

			var businessProfile = businessProfileList.Value;
			Result<IEnumerable<AffiliateAndSubsidiaryOutputDTO>> result = new Result<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>();
			var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Ownership.Id);
			var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);

			// Handle concurrency

			var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

			if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
			{
				var concurrencyCheck = ConcurrencyCheck(request.AffiliatesAndSubsidiariesConcurrencyToken, businessProfile);
				if (concurrencyCheck.Result.IsFailure)
				{
					return Result.Failure<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>(concurrencyCheck.Result.Error);
				}
			}

            if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete ||
				kycBusinessReviewResult != null))
			{
				//update
				result = await UpdateAffiliateAndSubsidiary(request, businessProfile, cancellationToken);

				if (result.IsFailure)
				{
					return Result.Failure<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>(
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
				result = await UpdateAffiliateAndSubsidiary(request, businessProfile, cancellationToken);

				if (result.IsFailure)
				{
					return Result.Failure<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>(
										$"Admin user is unable to update for {request.BusinessProfileCode}."
										);
				}

				//check mandatory fields
				//temporary commenting due to design issue on API causing deadlock
				//await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Ownership);
			}

			else
			{
				return Result.Failure<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>(
										 $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
										 );
			}

			return result;
		}

		private async Task<Result<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>> UpdateAffiliateAndSubsidiary(SaveAffliateAndSubsidiaryCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
			var subsidiary = await _businessProfileService.GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile);

			IReadOnlyList<AffiliateAndSubsidiary> subsidiaries =
				subsidiary.IsSuccess ? subsidiary.Value : Enumerable.Empty<AffiliateAndSubsidiary>().ToList().AsReadOnly();

			// To Add Values

			foreach (var inputSubsidiary in request.AffiliateAndSubsidiaries)
			{
				var country = await _businessProfileRepository.GetCountryMetaByISO2CodeAsync(inputSubsidiary.CountryISO2);


				if (inputSubsidiary.AffliateSubsidiaryCode == 0 || inputSubsidiary.AffliateSubsidiaryCode == null)
				{
					AffiliateAndSubsidiary affiliate =
						new AffiliateAndSubsidiary(businessProfile, inputSubsidiary.CompanyName,
						inputSubsidiary.CompanyRegNo, inputSubsidiary.DateOfIncorporation, country, inputSubsidiary.Relationship);

					await _businessProfileService.AddAffiliateAndSubsidiaryAsync(businessProfile, affiliate);

				}

				// To Update Records
				else if(!inputSubsidiary.IsDeleted)
				{
					var _ExistingAffiliate = subsidiaries
						.Where(i => i.Id == inputSubsidiary.AffliateSubsidiaryCode)
						.FirstOrDefault();

					if (_ExistingAffiliate == null)
					{
						return Result.Failure<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>(
								$"Invalid AffliateSubsidiaryCode [{inputSubsidiary.AffliateSubsidiaryCode}].");
					}

					_ExistingAffiliate.Country = country;
					_ExistingAffiliate.CompanyName = inputSubsidiary.CompanyName;
					_ExistingAffiliate.CompanyRegNo = inputSubsidiary.CompanyRegNo;
					_ExistingAffiliate.DateOfIncorporation = inputSubsidiary.DateOfIncorporation;
					_ExistingAffiliate.Relationship = inputSubsidiary.Relationship;

					await _businessProfileService.UpdateAffiliateAndSubsidiaryAsync(businessProfile, _ExistingAffiliate, cancellationToken);
				}
			}

			//To Delete Records

			var deletedSubsidiaries = from existing in subsidiaries
									  let fromInput = request.AffiliateAndSubsidiaries
									  .FirstOrDefault(input =>
										input.IsDeleted &&
										input.AffliateSubsidiaryCode.HasValue &&
										input.AffliateSubsidiaryCode.Value == existing.Id
										)
									  where fromInput?.AffliateSubsidiaryCode == existing?.Id
									  select existing;

			if (deletedSubsidiaries.Any())
			{
				List<long> deletedSubsidiariesIds = deletedSubsidiaries.Select(existing => existing.Id).ToList();

				await _screeningRepository.DeleteScreeningInputsByMyPropertyIds(deletedSubsidiariesIds, businessProfile.Id);
				await _businessProfileService.DeleteAffiliateAndSubsidiaryAsync(businessProfile, deletedSubsidiaries, cancellationToken);
			}

			subsidiary = await _businessProfileService.GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile);
			var _isSubsidiaryCompleted = await _businessProfileService.IsOwnershipSubsidiaryCompleted(businessProfile.Id);

			// Check if any Affiliates remain after updates or deletions
			var remainingAffils = await _businessProfileService.GetAffiliateAndSubsidiaryByBusinessProfileCodeAsync(businessProfile);

			// After updates or deletions
			if (remainingAffils.IsSuccess && !remainingAffils.Value.Any())
			{
				// All Affiliates are deleted, set the concurrency token to null
				businessProfile.AffiliatesAndSubsidiariesConcurrencyToken = null;
			}
			else
			{
				// Update the concurrency token since some Affiliates are still present
				businessProfile.AffiliatesAndSubsidiariesConcurrencyToken = Guid.NewGuid();
			}

			// Update the business profile
			await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

			return Result.Success<IEnumerable<AffiliateAndSubsidiaryOutputDTO>>(subsidiary.Value.Select(s => new AffiliateAndSubsidiaryOutputDTO()
			{
				CompanyRegNo = s.CompanyRegNo,
				CompanyName = s.CompanyName,
				CountryISO2 = s.Country?.CountryISO2,
				DateOfIncorporation = s.DateOfIncorporation,
				AffliateSubsidiaryCode = s.Id,
				isCompleted = _isSubsidiaryCompleted,
				Relationship = s.Relationship,
				AffiliatesAndSubsidiariesConcurrencyToken = businessProfile.AffiliatesAndSubsidiariesConcurrencyToken
			}));
		}

		private async Task<Result> ConcurrencyCheck(Guid? concurrencyToken, BusinessProfile businessProfile)
		{
			try
			{
				if ((concurrencyToken.HasValue && businessProfile.AffiliatesAndSubsidiariesConcurrencyToken != concurrencyToken) ||
					concurrencyToken is null && businessProfile.AffiliatesAndSubsidiariesConcurrencyToken != null)
				{
					// Return a 409 Conflict status code when there's a concurrency issue
					return Result.Failure<AffiliateAndSubsidiaryOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
				}

				// Stamp new token
				businessProfile.AffiliatesAndSubsidiariesConcurrencyToken = Guid.NewGuid();
				await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

				return Result.Success();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "An error occurred while processing the request.");

				// Return a 409 Conflict status code
				return Result.Failure<AffiliateAndSubsidiaryOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
			}
		}
	}
}

