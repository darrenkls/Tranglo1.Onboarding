using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.ExternalServices.Watchlist;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    [Permission(Permission.KYCManagementDeclaration.Action_Submit_Review_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementDeclaration.Action_View_Code })]
    internal class SubmitBusinessUserKYCCommand : BaseCommand<Result<SubmitBusinessKYCOutputDTO>>
    {
        public int BusinessProfileCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }
        public string LoginId { get; set; }
        public string EntityCode { get; set; }
        public Guid? ReviewConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<SubmitBusinessKYCOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"User submitted KYC for approval";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SubmitBusinessUserKYCCommandHandler : IRequestHandler<SubmitBusinessUserKYCCommand, Result<SubmitBusinessKYCOutputDTO>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly RBAService _rbaService;
        private readonly ILogger<SubmitBusinessUserKYCCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly IBusinessProfileRepository _repository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ComplianceScreeningService _complianceScreeningService;
        private readonly IWatchlistNotificationExternalService _watchlistNotificationExternalService;

        public SubmitBusinessUserKYCCommandHandler(
            BusinessProfileService businessProfileService,
            RBAService rbaService,
            ILogger<SubmitBusinessUserKYCCommandHandler> logger,
            TrangloUserManager userManager,
            IBusinessProfileRepository repository,
            IPartnerRepository partnerRepository,
            ComplianceScreeningService complianceScreeningService,
            IWatchlistNotificationExternalService watchlistNotificationExternalService)
        {
            _businessProfileService = businessProfileService;
            _rbaService = rbaService;
            _logger = logger;
            _userManager = userManager;
            _repository = repository;
            _partnerRepository = partnerRepository;
            _complianceScreeningService = complianceScreeningService;
            _watchlistNotificationExternalService = watchlistNotificationExternalService;
        }

        public async Task<Result<SubmitBusinessKYCOutputDTO>> Handle(SubmitBusinessUserKYCCommand request, CancellationToken cancellationToken)
        {
            var businessProfile = await _repository.GetBusinessProfileByCodeAsync(request.BusinessProfileCode);
            var result = Result.Success(new SubmitBusinessKYCOutputDTO());
            try
            {
                Guid? concurrencyToken = request.ReviewConcurrencyToken;

                if ((concurrencyToken.HasValue && businessProfile.ReviewConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && businessProfile.ReviewConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure<SubmitBusinessKYCOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                if (businessProfile.ReviewConcurrencyToken == null && businessProfile.ReviewConcurrencyToken == null)
                {
                    // Handle the scenario of fresh data here
                    businessProfile.ReviewConcurrentLastModified = DateTime.UtcNow;
                    businessProfile.ReviewConcurrencyToken = Guid.NewGuid();

                    // Update the data asynchronously
                    await _repository.UpdateBusinessProfileAsync(businessProfile);
                }
                else
                {
                    // Handle the scenario where ConcurrencyToken is provided
                    businessProfile.ReviewConcurrentLastModified = DateTime.UtcNow;
                    businessProfile.ReviewConcurrencyToken = Guid.NewGuid();

                    // Update the data asynchronously
                    await _repository.UpdateBusinessProfileAsync(businessProfile);
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code with an appropriate error message
                return Result.Failure<SubmitBusinessKYCOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
            }





            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (ClaimCode.Connect == request.CustomerSolution)
                {
                    return Result.Failure<SubmitBusinessKYCOutputDTO>(
                        $"Connect Customer user is unable to update for {request.BusinessProfileCode}."
                    );
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    result = await SubmitBusinessKYC(request, businessProfile, request.EntityCode, Solution.Business.Id, businessProfile.CollectionTier);

                    if (result.IsFailure)
                    {
                        return Result.Failure<SubmitBusinessKYCOutputDTO>(
                            $"Unable to submit the KYC. {result.Error}"
                        );
                    }
                }
                else if (Solution.Connect.Id == request.AdminSolution)
                {
                    return Result.Failure<SubmitBusinessKYCOutputDTO>(
                        $"Admin user is unable to update for Connect User with Business Profile: {request.BusinessProfileCode}."
                    );
                }
                else if (Solution.Business.Id == request.AdminSolution)
                {
                    result = await SubmitBusinessKYC(request, businessProfile, request.EntityCode, Solution.Business.Id, businessProfile.CollectionTier);

                    if (result.IsFailure)
                    {
                        return Result.Failure<SubmitBusinessKYCOutputDTO>(
                            $"Admin user is unable to update for {request.BusinessProfileCode}. {result.Error}"
                        );
                    }
                }
                else
                {
                    return Result.Failure<SubmitBusinessKYCOutputDTO>(
                        $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                    );
                }

                return result;
            }
            else
            {

                return Result.Failure<SubmitBusinessKYCOutputDTO>("Invalid request");
            }
        }

        private async Task<Result<SubmitBusinessKYCOutputDTO>> SubmitBusinessKYC(SubmitBusinessUserKYCCommand request, BusinessProfile businessProfile, string entityType, long? solution, CollectionTier collectionTier)
        {
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerInfo = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
            if (partnerInfo.CustomerTypeCode == CustomerType.Corporate_Normal_Corporate.Id)
            {
                var mandatoryFields = await _businessProfileService.IsNormalCorporateCustomerMandatoryFieldCompletedAsync(businessProfile.Id);

                var missingFields = new List<string>();

                if (!mandatoryFields.isBusinessProfileCompleted)
                    missingFields.Add("Business Profile");

                if (!mandatoryFields.isBusinessUserDeclarationInfoCompleted)
                    missingFields.Add("Business User Declaration");

                if (!mandatoryFields.isDocumentationCompleted)
                    missingFields.Add("Documentation");

                if (missingFields.Any())
                {
                    var missingFieldsStr = string.Join(", ", missingFields);
                    return Result.Failure<SubmitBusinessKYCOutputDTO>(
                        $"Mandatory field(s) not completed: {missingFieldsStr}."
                    );
                }


                // Handle specific checks based on customer type
                var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partner.CustomerType.Id);
                //entityType = entityType.ToUpper();

                bool _hasUploadedAMLDocumentation = await _businessProfileService.CheckHasUploadedBusinessAMLDocumentation(request.BusinessProfileCode, Solution.Business);
                //bool isAMLCFTComplete = true;
                //if (!_hasUploadedAMLDocumentation)
                //{
                //    isAMLCFTComplete = mandatoryFields.isAMLCompleted;
                //}

                if (businessProfile.BusinessKYCSubmissionStatus.Id != KYCSubmissionStatus.Submitted.Id)
                {
                    if ((customerType.Id == CustomerType.Corporate_Cryptocurrency_Exchange.Id ||
                         customerType.Id == CustomerType.Remittance_Partner.Id) && !mandatoryFields.isOwnershipCompleted)
                    {
                        return Result.Failure<SubmitBusinessKYCOutputDTO>($"Mandatory ownership field for {request.BusinessProfileCode} is not completed.");
                    }

                    if ((customerType.Id == CustomerType.Individual.Id ||
                         customerType.Id == CustomerType.Corporate_Normal_Corporate.Id ||
                         customerType.Id == CustomerType.Corporate_Mass_Payout.Id) &&
                        (!mandatoryFields.isBusinessProfileCompleted || !mandatoryFields.isBusinessUserDeclarationInfoCompleted))
                    {
                        return Result.Failure<SubmitBusinessKYCOutputDTO>($"Mandatory field for {request.BusinessProfileCode} is not completed. {mandatoryFields}");
                    }
                }

            }
            else
            {
                var mandatoryFieldsSummary = await _businessProfileService.IsBusinessCustomerMandatoryFieldCompletedAsync(businessProfile.Id);

                if (!mandatoryFieldsSummary.IsBusinessProfileCompleted || !mandatoryFieldsSummary.IsDeclarationInfoCompleted)
                {
                    return Result.Failure<SubmitBusinessKYCOutputDTO>(
                        $"Mandatory field for {request.BusinessProfileCode} is not completed."
                    );
                }

                // Handle specific checks based on customer type
                var partner = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(businessProfile.Id);
                var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partner.CustomerType.Id);
                entityType = entityType.ToUpper();

                bool _hasUploadedAMLDocumentation = await _businessProfileService.CheckHasUploadedBusinessAMLDocumentation(request.BusinessProfileCode, Solution.Business);
                bool isAMLCFTComplete = true;
                if (!_hasUploadedAMLDocumentation)
                {
                    isAMLCFTComplete = mandatoryFieldsSummary.IsAMLCompleted;
                }

                if (businessProfile.BusinessKYCSubmissionStatus.Id != KYCSubmissionStatus.Submitted.Id)
                {
                    if ((customerType.Id == CustomerType.Corporate_Cryptocurrency_Exchange.Id ||
                         customerType.Id == CustomerType.Remittance_Partner.Id) && !mandatoryFieldsSummary.IsOwnershipCompleted)
                    {
                        return Result.Failure<SubmitBusinessKYCOutputDTO>($"Mandatory ownership field for {request.BusinessProfileCode} is not completed.");
                    }

                    if ((customerType.Id == CustomerType.Individual.Id ||
                         customerType.Id == CustomerType.Corporate_Normal_Corporate.Id ||
                         customerType.Id == CustomerType.Corporate_Mass_Payout.Id) &&
                        (!mandatoryFieldsSummary.IsBusinessProfileCompleted || !mandatoryFieldsSummary.IsDeclarationInfoCompleted))
                    {
                        return Result.Failure<SubmitBusinessKYCOutputDTO>($"Mandatory field for {request.BusinessProfileCode} is not completed. {mandatoryFieldsSummary}");
                    }
                }
            }



            var updateSuccess = await _businessProfileService.SubmitBusinessKYCAsync(request.BusinessProfileCode, solution, collectionTier, request.CustomerSolution, applicationUser);

            if (updateSuccess)
            {

                if (businessProfile.BusinessKYCSubmissionStatusCode == KYCSubmissionStatus.Submitted.Id)
                {
                    var kycSubmissionStatus = await _repository.GetBusinessKYCSubmissionStatusBySubmissionStatusCode(businessProfile.KYCSubmissionStatusCode);

                    try
                    {
                        var complianceScreeningResult = await _complianceScreeningService.ScreeningAsync(businessProfile.Id);
                        if (complianceScreeningResult.IsSuccess)
                        {
                            // Send watchlist notification and process RBA
                            await _watchlistNotificationExternalService.SendAsync(
                                changeDTOs: complianceScreeningResult.Value.ChangeDTOs,
                                isSingleProfileScreening: true,
                                singlePartnerName: businessProfile.CompanyName);
                            await _rbaService.ProcessRiskEvaluationsAsync(complianceScreeningResult.Value.SingleScreeningListResultOutputDTOs, businessProfile.Id);
                        }

                        if (businessProfile.BusinessKYCSubmissionStatus == KYCSubmissionStatus.Draft)
                        {
                            businessProfile.BusinessKYCSubmissionStatus = KYCSubmissionStatus.Submitted;
                            await _repository.UpdateBusinessProfileAsync(businessProfile);
                        }

                        // Create the DTO with the required data
                        var outputDTO = new SubmitBusinessKYCOutputDTO
                        {
                            ReviewConcurrentLastModified = businessProfile.ReviewConcurrentLastModified,
                            ReviewConcurrencyToken = businessProfile.ReviewConcurrencyToken
                        };

                        return Result.Success(outputDTO); // Successful operation
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while processing business profile KYC and screening.");
                        return Result.Failure<SubmitBusinessKYCOutputDTO>($"Unable to Submit for KYC Review and Screening.");
                    }
                }
                else
                {
                    _logger.LogWarning("Business profile KYC submission was not successful.");
                    return Result.Failure<SubmitBusinessKYCOutputDTO>($"Business profile KYC submission was not successful.");
                }
            }

            return Result.Failure<SubmitBusinessKYCOutputDTO>($"Unable to Submit for KYC Review for Business Profile");
        }
    }
}



