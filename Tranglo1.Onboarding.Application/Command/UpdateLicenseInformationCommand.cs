using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Common;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.Specifications.BusinessProfiles;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCLicenseInformation, UACAction.Edit)]
    [Permission(Permission.KYCManagementLicenseInformation.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementLicenseInformation.Action_View_Code })]
    internal class UpdateLicenseInformationCommand : BaseCommand<Result<LicenseInformation>>
    {
        public string LoginId { get; set; }
        public bool? IsLicenseRequired { get; set; }
        public int BusinessProfileCode { get; set; }
        public int LicenseInformationCode { get; set; }
        public string LicenseType { get; set; }
        public string LicenseCertNumber { get; set; }
        public string PrimaryRegulatorLicenseService { get; set; }
        public string PrimaryRegulatorAMLCFT { get; set; }
        public string ActLawRemittanceLicense { get; set; }
        public string ActLawRemittanceAMLCFT { get; set; }
        public DateTime? IssuedDatetime { get; set; }
        public DateTime? ExpiredDatetime { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string Remark { get; set; }
        public string RegulatorWebsite { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        //Concurrency Token
        public Guid? LicenseInfoConcurrencyToken { get; set; }

        //TBT-1322
        public bool FromComment { get; set; }

        public override Task<string> GetAuditLogAsync(Result<LicenseInformation> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update License Information for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class UpdateLicenseInformationHandler : IRequestHandler<UpdateLicenseInformationCommand, Result<LicenseInformation>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly ILogger<UpdateLicenseInformationHandler> _logger;
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IPartnerRepository _partnerRepository;
            private readonly IConfiguration _config;

            public UpdateLicenseInformationHandler(ILogger<UpdateLicenseInformationHandler> logger, BusinessProfileService businessProfileService,
                TrangloUserManager userManager, PartnerService partnerService, IBusinessProfileRepository businessProfileRepository,
                IPartnerRepository partnerRepository, IConfiguration config)
            {
                _businessProfileService = businessProfileService;
                _logger = logger;
                _userManager = userManager;
                _partnerService = partnerService;
                _businessProfileRepository = businessProfileRepository;
                _partnerRepository = partnerRepository;
                _config = config;
            }

            public async Task<Result<LicenseInformation>> Handle(UpdateLicenseInformationCommand request, CancellationToken cancellationToken)
            {
                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
                var licenseInfo = await _businessProfileService.GetLicenseInfoByBusinessCode(request.BusinessProfileCode);
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

                if (licenseInfo == null)

                {
                    return Result.Failure<LicenseInformation>(
                                $"S No record found for businessProfileCode {request.BusinessProfileCode}"
                            );
                }

                var businessProfilesResult = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
                BusinessProfile businessProfile = businessProfilesResult.Value;

                // Handle concurrency
                var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
                {
                    var concurrencyCheck = ConcurrencyCheck(request.LicenseInfoConcurrencyToken, licenseInfo);
                    if (concurrencyCheck.IsFailure)
                    {
                        return Result.Failure<LicenseInformation>(concurrencyCheck.Error);
                    }
                }

                var expiredDateRequest = request.ExpiredDatetime;
                DateTime currentDate = DateTime.Today;
                TimeSpan? diff = currentDate - expiredDateRequest;
                int? daysDiff = (int?)diff?.TotalDays;
                if (daysDiff >= 365)
                {
                    return Result.Failure<LicenseInformation>(
                               $"S License Expiry is not valid for {request.BusinessProfileCode}."
                           );
                }

                licenseInfo.BusinessProfileCode = request.BusinessProfileCode;
                licenseInfo.IsLicenseRequired = request.IsLicenseRequired;
                licenseInfo.LicenseType = request.LicenseType;
                licenseInfo.LicenseCertNumber = request.LicenseCertNumber;
                licenseInfo.IssuedDate = request.IssuedDatetime;
                licenseInfo.ExpiryDate = expiredDateRequest;
                licenseInfo.PrimaryRegulatorLicenseService = request.PrimaryRegulatorLicenseService;
                licenseInfo.PrimaryRegulatorAMLCFT = request.PrimaryRegulatorAMLCFT;
                licenseInfo.ActLawAMLCFT = request.ActLawRemittanceAMLCFT;
                licenseInfo.ActLawRemittanceLicense = request.ActLawRemittanceLicense;
                licenseInfo.Remark = request.Remark;
                licenseInfo.RegulatorWebsite = request.RegulatorWebsite;

                Result<LicenseInformation> result = new Result<LicenseInformation>();
                var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_LicenseInfo.Id);

                if (request.AdminSolution != null || request.CustomerSolution != null)
                {
                    if (ClaimCode.Connect == request.CustomerSolution)
                    {
                        if (applicationUser is CustomerUser && (businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && kycReviewResult == ReviewResult.Insufficient_Incomplete) || request.CustomerSolution == ClaimCode.Business)
                        {
                            //update
                            result = await UpdateLicenseInformation(request, businessProfile, licenseInfo);

                            if (result.IsFailure)
                            {
                                return Result.Failure<LicenseInformation>(
                                                    $"Customer user is unable to update for {request.BusinessProfileCode}."
                                                    );
                            }

                        }
                    }
                    else if (ClaimCode.Business == request.CustomerSolution)
                    {
                        result = await UpdateLicenseInformation(request, businessProfile, licenseInfo);

                        if (result.IsFailure)
                        {
                            return Result.Failure<LicenseInformation>(
                                                $"Customer user is unable to update for {request.BusinessProfileCode}."
                                                );
                        }

                        if (request.FromComment)
                        {
                            var kycSummaryFeedbackInfo = await _businessProfileRepository.GetListKYCSummaryFeedbackByBusinessProfileCodeAsync(request.BusinessProfileCode);

                            foreach (var i in kycSummaryFeedbackInfo)
                            {
                                if (i.IsResolved == false && i.KYCCategory.Id == KYCCategory.Business_LicenseInfo.Id)
                                {
                                    i.IsResolved = true; //set isResolved to true
                                    await _businessProfileRepository.SaveKYCSummaryFeedback(i);
                                }
                            }
                        }

                        await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode,
                            KYCCategory.Business_LicenseInfo.Id,
                            cancellationToken);
                    }
                    else if (Solution.Connect.Id == request.AdminSolution)
                    {
                        if (applicationUser is TrangloStaff &&
                           ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                           businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || kycReviewResult == ReviewResult.Complete) || request.AdminSolution.Value == Solution.Business.Id)
                        {
                            //update
                            result = await UpdateLicenseInformation(request, businessProfile, licenseInfo);

                            if (result.IsFailure)
                            {
                                return Result.Failure<LicenseInformation>(
                                                    $"Admin user is unable to update for {request.BusinessProfileCode}."
                                                    );
                            }

                            //check mandatory fields
                            await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_LicenseInfo);
                        }
                    }
                    else if (Solution.Business.Id == request.AdminSolution)
                    {
                        //update
                        result = await UpdateLicenseInformation(request, businessProfile, licenseInfo);

                        if (result.IsFailure)
                        {
                            return Result.Failure<LicenseInformation>(
                                                $"Admin user is unable to update for {request.BusinessProfileCode}."
                                                );
                        }

                        //check mandatory fields
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_LicenseInfo);
                    }
                    else
                    {
                        return Result.Failure<LicenseInformation>(
                                                 $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                                 );
                    }
                }
                else
                {
                    return Result.Failure<LicenseInformation>($"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
                }
                return result;
            }

            private async Task<Result<LicenseInformation>> UpdateLicenseInformation(UpdateLicenseInformationCommand request, BusinessProfile businessProfile, LicenseInformation licenseInfo)
            {
                Result<LicenseInformation> UpdateLicenseInformationResp = await _businessProfileService.UpdateLicenseInformationsAsync(businessProfile, licenseInfo);
                if (UpdateLicenseInformationResp.IsFailure)
                {
                    _logger.LogError($"[UpdateLicenseInformationCommand] {UpdateLicenseInformationResp.Error}");

                    return Result.Failure<LicenseInformation>(
                                $"Update License Information failed for {request.BusinessProfileCode}."
                            );
                }

                return Result.Success(UpdateLicenseInformationResp.Value);
            }

            private Result ConcurrencyCheck(Guid? concurrencyToken, LicenseInformation licenseInfo)
            {
                try
                {
                    if ((concurrencyToken.HasValue && licenseInfo.LicenseInfoConcurrencyToken != concurrencyToken) ||
                        concurrencyToken is null && licenseInfo.LicenseInfoConcurrencyToken != null)
                    {
                        // Return a 409 Conflict status code when there's a concurrency issue
                        return Result.Failure<LicenseInformation>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                    }

                    // Stamp new token
                    licenseInfo.LicenseInfoConcurrencyToken = Guid.NewGuid();
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while processing the request.");

                    // Return a 409 Conflict status code
                    return Result.Failure<LicenseInformation>("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
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
                    _logger.LogError(ex, "[{0}]", nameof(UpdateLicenseInformationHandler));
                }
            }
        }
    }
}