using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.ExternalServices.Watchlist;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.Services.Notification;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    [Permission(Permission.KYCManagementDeclaration.Action_Submit_Review_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect },
        new string[] { Permission.KYCManagementDeclaration.Action_View_Code })]
    internal class SubmitKYCCommand : BaseCommand<Result<SubmitKYCOutputDTO>>
    {
        public int businessProfileCode { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public Guid? ReviewConcurrencyToken { get; set; }



        public override Task<string> GetAuditLogAsync(Result<SubmitKYCOutputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Submit KYC for Business Profile Code: [{this.businessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SubmitKYCCommandHandler : IRequestHandler<SubmitKYCCommand, Result<SubmitKYCOutputDTO>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly PartnerService _partnerService;
        private readonly TrangloUserManager _userManager;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly ComplianceScreeningService _complianceScreeningService;
        private readonly IWatchlistNotificationExternalService _watchlistNotificationExternalService;

        public SubmitKYCCommandHandler(BusinessProfileService businessProfileService,
                                       PartnerService partnerService,
                                       TrangloUserManager userManager,
                                       IBusinessProfileRepository businessProfileRepository,
                                       IPartnerRepository partnerRepository,
                                       ComplianceScreeningService complianceScreeningService,
                                       IWatchlistNotificationExternalService watchlistNotificationExternalService)
        {
            _businessProfileService = businessProfileService;
            _partnerService = partnerService;
            _userManager = userManager;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
            _complianceScreeningService = complianceScreeningService;
            _watchlistNotificationExternalService = watchlistNotificationExternalService;
        }

        public async Task<Result<SubmitKYCOutputDTO>> Handle(SubmitKYCCommand request, CancellationToken cancellationToken)
        {
            var solution = request.CustomerSolution;
            var businessProfile = await _businessProfileRepository.GetBusinessProfileByCodeAsync(request.businessProfileCode);
            var partnerInfo = await _partnerRepository.GetPartnerRegistrationByBusinessProfileCodeAsync(request.businessProfileCode);
            var partnerSubInfo = await _partnerRepository.GetPartnerSubscriptionByPartnerCodeAsync(partnerInfo.Id);
            DateTime? currentLastModified = await _businessProfileRepository.GetLastReviewConcurrentModifiedTimestamp(businessProfile.Id);

           // try
           // {
           //     Guid? reviewConcurrencyToken = request.ReviewConcurrencyToken;
           //
           //     if ((reviewConcurrencyToken.HasValue && businessProfile.ReviewConcurrencyToken != reviewConcurrencyToken) ||
           //         reviewConcurrencyToken is null && businessProfile.ReviewConcurrencyToken != null)
           //     {
           //         // Return a 409 Conflict status code when there's a concurrency issue
           //         return Result.Failure<SubmitKYCOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
           //     }
           //
           //     if (businessProfile.ReviewConcurrencyToken == null)
           //     {
           //         // Handle the scenario of fresh data here
           //         businessProfile.ReviewConcurrentLastModified = DateTime.UtcNow;
           //         businessProfile.ReviewConcurrencyToken = Guid.NewGuid();
           //     }
           //     else
           //     {
           //         // Handle the scenario where ConcurrencyToken is provided
           //         businessProfile.ReviewConcurrentLastModified = DateTime.UtcNow;
           //         businessProfile.ReviewConcurrencyToken = Guid.NewGuid();
           //     }
           //
           // }
           // catch (Exception ex)
           // {
           //     Log.Error(ex, "An error occurred while processing the request.");
           //
           //     // Return a 409 Conflict status code with an appropriate error message
           //     return Result.Failure<SubmitKYCOutputDTO>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
           // }

            KYCSummary _isMandatoryFieldCompleted = new KYCSummary();
            bool _hasUploadedAMLDocumentation = false;
            
            _isMandatoryFieldCompleted = await _businessProfileService.IsMandatoryFieldCompletedAsync(request.businessProfileCode, Solution.Connect);
           _hasUploadedAMLDocumentation = await _businessProfileService.CheckHasUploadedAMLDocumentation(request.businessProfileCode, Solution.Connect);
            

            // Update BusinessProfile for new ConcurrencyToken. Only do this after isMandatoryFieldCompletedAsync to avoid separate connection issue (Db locking)
            await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfile);

            bool isAMLCFTComplete = true;
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.businessProfileCode);
            var businessProfileInfo = await _businessProfileRepository.GetBusinessProfileByCodeAsync(request.businessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);


            if (!_hasUploadedAMLDocumentation)
            {
                isAMLCFTComplete = _isMandatoryFieldCompleted.isAMLCompleted;
            }

            if (_isMandatoryFieldCompleted != null)
            {
                if(  _isMandatoryFieldCompleted.isBusinessProfileCompleted &&
                    _isMandatoryFieldCompleted.isCoInfoCompleted && isAMLCFTComplete && 
                    _isMandatoryFieldCompleted.isDeclarationInfoCompleted &&
                    _isMandatoryFieldCompleted.isDocumentationCompleted && _isMandatoryFieldCompleted.isOwnershipCompleted &&
                    _isMandatoryFieldCompleted.isLicenseInfoCompleted)
                {
                    var updateSuccess=await _businessProfileService.SubmitKycAsync(request.businessProfileCode, Solution.Connect.Id, businessProfileInfo.CollectionTier, request.CustomerSolution, applicationUser);
                    var kycSubmissionStatus = await _businessProfileRepository.GetBusinessKYCSubmissionStatusBySubmissionStatusCode(businessProfileInfo.KYCSubmissionStatusCode);

                    if (updateSuccess)
                    {
                        
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
                                //await _rbaService.ProcessRiskEvaluationsAsync(complianceScreeningResult.Value.SingleScreeningListResultOutputDTOs);
                            }

                            if (businessProfileInfo.BusinessKYCSubmissionStatus == KYCSubmissionStatus.Draft)
                            {
                                kycSubmissionStatus = KYCSubmissionStatus.Submitted;
                                await _businessProfileRepository.UpdateBusinessProfileAsync(businessProfileInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "An error occurred while processing business profile KYC and screening.");
                            return Result.Failure<SubmitKYCOutputDTO>($"Unable to Submit for KYC Review and Screening.");
                        }

                        var outputDTO = new SubmitKYCOutputDTO
                        {
                            ReviewConcurrentLastModified = businessProfile.ReviewConcurrentLastModified,
                            ReviewConcurrencyToken = businessProfile.ReviewConcurrencyToken
                        };
                        return Result.Success(outputDTO); // Successful operation
                    }

                    else
                    {
                        return Result.Failure<SubmitKYCOutputDTO>(
                             $"Unable to update the status for {request.businessProfileCode} "
                         );
                    }
                }
                else
                {
                   return Result.Failure<SubmitKYCOutputDTO>(
                             $"KYC is not completed. Please complete KYC before submit for review."
                         );
                }
            }

            var outputDTOs = new SubmitKYCOutputDTO
            {
                ReviewConcurrentLastModified = businessProfile.ReviewConcurrentLastModified,
                ReviewConcurrencyToken = businessProfile.ReviewConcurrencyToken
            };
            return Result.Success<SubmitKYCOutputDTO>(outputDTOs);
        }
    }
}
