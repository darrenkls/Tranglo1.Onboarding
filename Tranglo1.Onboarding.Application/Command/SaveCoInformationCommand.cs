using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Serilog;
using Microsoft.Extensions.Configuration;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCCOInformation, UACAction.Edit)]
    [Permission(Permission.KYCManagementCOInformation.Action_Edit_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementCOInformation.Action_View_Code })]
    internal class SaveCoInformationCommand : BaseCommand<Result<long>>
    {
        public string LoginId { get; internal set; }
        public string ComplianceOfficer { get; set; }
        public string PositionTitle { get; set; }
        public string CompanyAddress { get; set; }
        public string ZipCodePostCode { get; set; }
        public int CallingCode { get; set; }
        public string ContactNum { get; set; }
        public string EmailAddress { get; set; }
        public string ReportingTo { get; set; }
        public bool? IsRegisteredRegulator { get; set; }
        public bool? IsCertifiedByAML { get; set; }
        public string CertificationProgram { get; set; }
        public string CertificationBodyOrganization { get; set; }
        public int BusinessProfileCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public string DialCode { get; set; }
        public string ContactNumberCountryISO2 { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        // Concurrency Token
        public Guid? COInformationConcurrencyToken { get; set; }

        public override Task<string> GetAuditLogAsync(Result<long> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Save CO Information for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

        public class SaveCoInformationCommandHandler : IRequestHandler<SaveCoInformationCommand, Result<long>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly ILogger<SaveCoInformationCommandHandler> _logger;
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IPartnerRepository _partnerRepository;
            private readonly IConfiguration _config;

            public SaveCoInformationCommandHandler(
                BusinessProfileService businessProfileService,
                TrangloUserManager userManager,
                ILogger<SaveCoInformationCommandHandler> logger,
                PartnerService partnerService,
                IPartnerRepository partnerRepository,
                IBusinessProfileRepository businessProfileRepository,
                IConfiguration config
                )
            {
                _businessProfileService = businessProfileService;
                _logger = logger;
                _userManager = userManager;
                _businessProfileRepository = businessProfileRepository;
                _partnerService = partnerService;
                _partnerRepository = partnerRepository;
                _config = config;

            }

            public async Task<Result<long>> Handle(SaveCoInformationCommand request, CancellationToken cancellationToken)
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);


                var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
                BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();

                var COInfo = await _businessProfileService.GetCOInfoByBusinessCode(request.BusinessProfileCode);
                if (COInfo != null)
                { 
                    if (COInfo.ComplianceOfficer != null || COInfo.PositionTitle != null || COInfo.ReportingTo != null || COInfo.IsRegisteredRegulator != null || COInfo.IsCertifiedByAML != null)
                    {
                        return Result.Failure<long>(
                               $" Theres an existing record for Partner {businessProfile.CompanyName}."
                           );
                    }

                    // Handle concurrency

                    var tcRevampFeature = _config.GetValue<bool>("TCRevampFeature");

                    if ((request.CustomerSolution == ClaimCode.Connect || request.AdminSolution == Solution.Connect.Id) && tcRevampFeature == true)
                    {
                        var concurrencyCheck = ConcurrencyCheck(request.COInformationConcurrencyToken, COInfo);
                        if (concurrencyCheck.IsFailure)
                        {
                            return Result.Failure<long>(concurrencyCheck.Error);
                        }
                    }
                }

                // Generate a new concurrency token for first-time creation or when COInfo is null
                Guid newConcurrencyToken = COInfo?.COInformationConcurrencyToken ?? Guid.NewGuid();

                if (request.ContactNum != null || request.ContactNum != "")
                {
                    Result<ContactNumber> createContactNumber = string.IsNullOrWhiteSpace(request.ContactNum) || string.IsNullOrWhiteSpace(request.DialCode) ? null : ContactNumber.Create(request.DialCode, request.ContactNumberCountryISO2, request.ContactNum);
                    if (createContactNumber.IsFailure)
                    {
                        return Result.Failure<long>(
                                    $"Contact number failed for {request.ContactNum}. {createContactNumber.Error}"
                                );
                    }
                }                

                Result<Email> createEmail = string.IsNullOrWhiteSpace(request.EmailAddress) ? null : Email.Create(request.EmailAddress);
                if (createEmail.IsFailure)
                {
                    return Result.Failure<long>(
                                $"Email failed for {request.EmailAddress}. {createEmail.Error}"
                            );
                }


                COInformation CoInfo = new COInformation(businessProfile)
                {

                    ComplianceOfficer = request.ComplianceOfficer,
                    PositionTitle = request.PositionTitle,
                    CompanyAddress = request.CompanyAddress,
                    ZipCodePostCode = request.ZipCodePostCode,
                    CallingCode = request.CallingCode,
                    ContactNumber = string.IsNullOrWhiteSpace(request.ContactNum) ? null : ContactNumber.Create(request.DialCode, request.ContactNumberCountryISO2, request.ContactNum).Value,
                    EmailAddress = string.IsNullOrWhiteSpace(request.EmailAddress) ? null : Email.Create(request.EmailAddress).Value,
                    ReportingTo = request.ReportingTo,
                    IsRegisteredRegulator = request.IsRegisteredRegulator,
                    IsCertifiedByAML = request.IsCertifiedByAML,
                    CertificationProgram = request.CertificationProgram,
                    CertificationBodyOrganization = request.CertificationBodyOrganization,
                    COInformationConcurrencyToken = newConcurrencyToken
                };

                //add new checking #41516
                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
                var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_ComplianceInfo.Id);


                if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
                {

                    if ((applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && kycReviewResult == ReviewResult.Insufficient_Incomplete)
                        || (applicationUser is TrangloStaff &&
                            ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                            businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || kycReviewResult == ReviewResult.Complete)))
                    {
                        var addCOInformationResp = await _businessProfileService.AddCOInformationsAsync(businessProfile, CoInfo);
                        if (addCOInformationResp.IsFailure)
                        {
                            _logger.LogError($"[SaveCustomerCoInformationCommand] {addCOInformationResp.Error}");

                            return Result.Failure<long>(
                                        $"Save Customer Compliance Officers Info failed for {request.BusinessProfileCode}."
                                    );
                        }

                        return Result.Success<long>(addCOInformationResp.Value.Id);
                    }
                    else
                    {
                        return Result.Failure<long>(
                                $"Unable to save Customer Complaince Officers Info for {request.BusinessProfileCode}. Check failed"
                            );

                    }
                }

                if (ClaimCode.Business == request.CustomerSolution || Solution.Business.Id == request.AdminSolution)
                
                    {
                    var customerType = await _partnerRepository.GetCustomerTypeByCodeAsync(partnerRegistrationInfo.CustomerTypeCode.Value);
                    if (customerType == CustomerType.Corporate_Cryptocurrency_Exchange || customerType == CustomerType.Remittance_Partner)
                    {
                        var addCOInformationResp = await _businessProfileService.AddCOInformationsAsync(businessProfile, CoInfo);
                        if (addCOInformationResp.IsFailure)
                        {
                            _logger.LogError($"[SaveBusinessCoInformationCommand] {addCOInformationResp.Error}");

                            return Result.Failure<long>(
                                        $"Save Business Compliance Officers Info failed for {request.BusinessProfileCode}."
                                    );
                        }

                        return Result.Success<long>(addCOInformationResp.Value.Id);
                    }
                    else
                    {
                        return Result.Failure<long>(
                            $"Unable to save Business Complaince Officers Info for {request.BusinessProfileCode}. Customer Type not supported"
                        );
                    }
                       

                    }
                else
                {
                    return Result.Failure<long>(
                        $"Unable to save Business Complaince Officers Info for {request.BusinessProfileCode}. Check failed"
                    );
                }

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
}
