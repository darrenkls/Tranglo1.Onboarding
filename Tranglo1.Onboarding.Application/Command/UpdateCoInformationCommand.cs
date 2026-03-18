using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using CSharpFunctionalExtensions;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Domain.Repositories;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCCOInformation, UACAction.Edit)]
    internal class UpdateCoInformationCommand : BaseCommand<Result<COInformation>>
    {
        public string LoginId { get; internal set; }
        public string ComplianceOfficer { get; set; }
        public string PositionTitle { get; set; }
        public string CompanyAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public int CallingCode { get; set; }
        public string ContactNumber { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        public string EmailAddress { get; set; }
        public string ReportingTo { get; set; }
        public bool? IsRegisteredRegulator { get; set; }
        public bool? IsCertifiedByAML { get; set; }
        public string CertificationProgram { get; set; }
        public string CertificationBodyOrganization { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public int BusinessProfileCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public string DialCode { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        // Concurrency Token
        public Guid? COInformationConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<COInformation> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update CO Information for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdateCoInformationCommandHandler : IRequestHandler<UpdateCoInformationCommand, Result<COInformation>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<UpdateCoInformationCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IConfiguration _config;

        public UpdateCoInformationCommandHandler(
                ILogger<UpdateCoInformationCommandHandler> logger,
                BusinessProfileService businessProfileService,
                TrangloUserManager userManager,
                PartnerService partnerService,
                IPartnerRepository partnerRepository,
                IBusinessProfileRepository businessProfileRepository,
                IConfiguration config
            )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
            _config = config;
        }

        public async Task<Result<COInformation>> Handle(UpdateCoInformationCommand request, CancellationToken cancellationToken)
        {
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var COInfo = await _businessProfileService.GetCOInfoByBusinessCode(request.BusinessProfileCode);
            if (COInfo == null)

            {
                return Result.Failure<COInformation>(
                            $"S No record found for {request.BusinessProfileCode}."
                        );
            }

            // Handle concurrency
            var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

            if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
            {
                var concurrencyCheck = ConcurrencyCheck(request.COInformationConcurrencyToken, COInfo);
                if (concurrencyCheck.IsFailure)
                {
                    return Result.Failure<COInformation>(concurrencyCheck.Error);
                }
            }

            if (request.ContactNumber != null || request.ContactNumber != "")
            {
                Result<ContactNumber> createContactNumber = string.IsNullOrWhiteSpace(request.ContactNumber) || string.IsNullOrWhiteSpace(request.DialCode) ? null : ContactNumber.Create(request.DialCode, request.ContactNumberCountryISO2, request.ContactNumber);
                if (createContactNumber.IsFailure)
                {
                    return Result.Failure<COInformation>(
                                $"Create contact number failed for {request.ContactNumber}. {createContactNumber.Error}"
                            );
                }

                COInfo.ContactNumber = createContactNumber.Value;
            }

            Result<Email> createEmail = string.IsNullOrWhiteSpace(request.EmailAddress) ? null : Email.Create(request.EmailAddress);
            if (createEmail.IsFailure)
            {
                return Result.Failure<COInformation>(
                            $"Email failed for {request.EmailAddress}. {createEmail.Error}"
                        );
            }

            COInfo.ComplianceOfficer = request.ComplianceOfficer;
            COInfo.PositionTitle = request.PositionTitle;
            COInfo.CompanyAddress = request.CompanyAddress;
            COInfo.ZipCodePostCode = request.ZipCodePostCode;
            COInfo.CallingCode = request.CallingCode;
            COInfo.EmailAddress = string.IsNullOrWhiteSpace(request.EmailAddress) ? null : Email.Create(request.EmailAddress).Value;
            COInfo.ReportingTo = request.ReportingTo;
            COInfo.IsRegisteredRegulator = request.IsRegisteredRegulator;
            COInfo.IsCertifiedByAML = request.IsCertifiedByAML;
            COInfo.CertificationProgram = request.CertificationProgram;
            COInfo.CertificationBodyOrganization = request.CertificationBodyOrganization;
           

            var businessProfilesResult = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            BusinessProfile businessProfile = businessProfilesResult.Value;

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            Result<COInformation> update = new Result<COInformation>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_ComplianceInfo.Id);



            if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
            {
                if ((applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && kycReviewResult == ReviewResult.Insufficient_Incomplete)
                     || (applicationUser is TrangloStaff &&
                         ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                         businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || kycReviewResult == ReviewResult.Complete)))

                    update = await UpdateCOInformation(request, businessProfile, COInfo);

                if (update.IsFailure)
                {
                    return Result.Failure<COInformation>(
                                        $"{update.Error}"
                                        );
                }

            }


            else if (ClaimCode.Business == request.CustomerSolution || Solution.Business.Id == request.AdminSolution)

            {
                var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerRegistrationInfo.CustomerTypeCode.Value);
                if (customerType == CustomerType.Corporate_Cryptocurrency_Exchange || customerType == CustomerType.Remittance_Partner)
                {
                    //update
                    update = await UpdateCOInformation(request, businessProfile, COInfo);

                    if (update.IsFailure)
                    {
                        return Result.Failure<COInformation>(
                                            $"{update.Error}"
                                            );
                    }

                    //check mandatory fields
                    await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_ComplianceInfo);
                }

                else
                {
                    return Result.Failure<COInformation>(
                                             $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}.Customer Type not supported"
                                             );
                }

            }
            else
            {
                return Result.Failure<COInformation>(
                                           $"Unable to update for BusinessProfileCode {request.BusinessProfileCode}."
                                           );
            }
            return update;                      
        }

        private async Task<Result<COInformation>> UpdateCOInformation(UpdateCoInformationCommand request, BusinessProfile businessProfile, COInformation COInfo)
        {
            Result<COInformation> UpdateCOInformationResp = await _businessProfileService.UpdateCOInformationsAsync(businessProfile, COInfo);
            if (UpdateCOInformationResp.IsFailure)
            {
                _logger.LogError($"[UpdateCoInformationCommand] {UpdateCOInformationResp.Error}");

                return Result.Failure<COInformation>(
                            $"Update Compliance Officers Info failed for {request.BusinessProfileCode}."
                        );
            }

            return Result.Success(UpdateCOInformationResp.Value);
        }

        private Result ConcurrencyCheck(Guid? concurrencyToken, COInformation coInformation)
        {
            try
            {
                if ((concurrencyToken.HasValue && coInformation.COInformationConcurrencyToken != concurrencyToken) ||
                    concurrencyToken is null && coInformation.COInformationConcurrencyToken != null)
                {
                    // Return a 409 Conflict status code when there's a concurrency issue
                    return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
                }

                // Stamp new token
                coInformation.COInformationConcurrencyToken = Guid.NewGuid();
                return Result.Success();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while processing the request.");

                // Return a 409 Conflict status code
                return Result.Failure("ConcurrencyError, Data has been modified by another user. Please refresh and try again.");
            }
        }
    }
}
