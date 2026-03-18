using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
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

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCLicenseInformation, UACAction.Edit)]
    internal class SaveLicenseInformationCommand : BaseCommand<Result<long>>
    {
        public string LoginId { get; internal set; }
        public int BusinessProfileCode { get; set; }
        public bool? IsLicenseRequired { get; set; }
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

        public override Task<string> GetAuditLogAsync(Result<long> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Save License Information for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class SaveLicenseInformationCommandHandler : IRequestHandler<SaveLicenseInformationCommand, Result<long>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly ILogger<SaveLicenseInformationCommandHandler> _logger;
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IConfiguration _config;
            public SaveLicenseInformationCommandHandler(
                    BusinessProfileService businessProfileService,
                    ILogger<SaveLicenseInformationCommandHandler> logger,
                    TrangloUserManager userManager,
                    PartnerService partnerService,
                    IBusinessProfileRepository businessProfileRepository,
                    IConfiguration config
                )
            {
                _businessProfileService = businessProfileService;
                _logger = logger;
                _userManager = userManager;
                _businessProfileRepository = businessProfileRepository;
                _partnerService = partnerService;
                _config = config;
            }

            public async Task<Result<long>> Handle(SaveLicenseInformationCommand request, CancellationToken cancellationToken)
            {
                var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
                BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();

                var checkLicenseInfo = await _businessProfileService.GetLicenseInfoByBusinessCode(request.BusinessProfileCode);
                if (checkLicenseInfo != null)
                {
                    return Result.Failure<long>(
                                $"S Theres an existing record for {request.BusinessProfileCode}."
                            );
                }

                var expiredDateRequest = request.ExpiredDatetime;
                DateTime currentDate = DateTime.Today;
                TimeSpan? diff = currentDate - expiredDateRequest;
                int? daysDiff = (int?)diff?.TotalDays;
                if (daysDiff >= 365)
                {
                    return Result.Failure<long>(
                               $"S License Expiry is not valid for {request.BusinessProfileCode}."
                           );
                }

                LicenseInformation licenseInfo = new LicenseInformation(businessProfile)
                {
                    BusinessProfileCode = request.BusinessProfileCode,
                    IsLicenseRequired = request.IsLicenseRequired,
                    LicenseType = request.LicenseType,
                    LicenseCertNumber = request.LicenseCertNumber,
                    IssuedDate = request.IssuedDatetime,
                    ExpiryDate = expiredDateRequest,
                    PrimaryRegulatorLicenseService = request.PrimaryRegulatorLicenseService,
                    PrimaryRegulatorAMLCFT = request.PrimaryRegulatorAMLCFT,
                    ActLawAMLCFT = request.ActLawRemittanceAMLCFT,
                    ActLawRemittanceLicense = request.ActLawRemittanceLicense,
                    Remark = request.Remark,
                    RegulatorWebsite = request.RegulatorWebsite
                };

                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
                var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_LicenseInfo.Id);

                // Handle concurrency

                var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
                {
                    var concurrencyCheck = ConcurrencyCheck(request.LicenseInfoConcurrencyToken, licenseInfo);
                    if (concurrencyCheck.IsFailure)
                    {
                        return Result.Failure<long>(concurrencyCheck.Error);
                    }
                }

                if (request.AdminSolution != null || request.CustomerSolution != null)
                {
                    if (Solution.Connect.Id == request.AdminSolution || ClaimCode.Connect == request.CustomerSolution)
                    {
                        if (((applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && kycReviewResult == ReviewResult.Insufficient_Incomplete) || request.CustomerSolution == ClaimCode.Business) ||
                        (applicationUser is TrangloStaff && ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) || businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || kycReviewResult == ReviewResult.Complete)
                        || request.AdminSolution == Solution.Business.Id))
                        {
                            var addLicenseInformationResp = await _businessProfileService.AddLicenseInformationsAsync(businessProfile, licenseInfo);
                            if (addLicenseInformationResp.IsFailure)
                            {
                                _logger.LogError($"[SaveLicenseInformationCommand] {addLicenseInformationResp.Error}");

                                return Result.Failure<long>(
                                            $"Save License Info failed for {request.BusinessProfileCode}."
                                        );
                            }

                            return Result.Success<long>(addLicenseInformationResp.Value.Id);
                        }
                        else
                        {
                            return Result.Failure<long>(
                                $"Save License Info failed for {request.BusinessProfileCode}. Check Failed"
                            );
                        }
                    }
                    else if (Solution.Business.Id == request.AdminSolution || ClaimCode.Business == request.CustomerSolution)
                    {
                        var addLicenseInformationResp = await _businessProfileService.AddLicenseInformationsAsync(businessProfile, licenseInfo);
                        if (addLicenseInformationResp.IsFailure)
                        {
                            _logger.LogError($"[SaveLicenseInformationCommand] {addLicenseInformationResp.Error}");

                            return Result.Failure<long>(
                                        $"Save License Info failed for {request.BusinessProfileCode}."
                                    );
                        }

                        if (ClaimCode.Business == request.CustomerSolution)
                        {
                            await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode,
                                KYCCategory.Business_LicenseInfo.Id,
                                cancellationToken);
                        }

                        return Result.Success<long>(addLicenseInformationResp.Value.Id);
                    }
                    else
                    {
                        return Result.Failure<long>(
                                $"Save License Info failed for {request.BusinessProfileCode}. Check Failed"
                            );
                    }

                }
                else
                {
                    return Result.Failure<long>(
                            $"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                        );
                }

            }

            private Result<LicenseInformation> ConcurrencyCheck(Guid? concurrencyToken, LicenseInformation licenseInfo)
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
                    return Result.Success(licenseInfo);
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
                    _logger.LogError(ex, "[{0}]", nameof(SaveLicenseInformationCommandHandler));
                }
            }
        }
    }
}