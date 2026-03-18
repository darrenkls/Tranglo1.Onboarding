using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
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
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDeclaration, UACAction.Edit)]
    internal class UpdateDeclarationInformationCommand : BaseCommand<Result<Declaration>>
    {
        public string LoginId { get; set; }
        public int BusinessProfileCode { get; set; }
        public bool? IsAuthorized { get; set; }
        public bool? IsInformationTrue { get; set; }
        public bool? IsAgreedTermsOfService { get; set; }
        public bool? IsDeclareTransactionTax { get; set; }
        public bool? IsAllApplicationAccurate { get; set; }
        public string SigneeName { get; set; }
        public string Designation { get; set; }

        public override Task<string> GetAuditLogAsync(Result<Declaration> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Declarations for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class UpdateDeclarationInformationCommandHandler : IRequestHandler<UpdateDeclarationInformationCommand, Result<Declaration>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<UpdateDeclarationInformationCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        public UpdateDeclarationInformationCommandHandler(BusinessProfileService businessProfileService,
                                                         ILogger<UpdateDeclarationInformationCommandHandler> logger,
                                                         TrangloUserManager userManager,
                                                         PartnerService partnerService,
                                                         IBusinessProfileRepository businessProfileRepository)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
        }
        public async Task<Result<Declaration>> Handle(UpdateDeclarationInformationCommand request, CancellationToken cancellationToken)
        {
            var _BusinessProfile = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);

            var businessProfile = _BusinessProfile.Value?.FirstOrDefault();

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Declaration.Id);

            if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft || kycReviewResult != ReviewResult.Insufficient_Incomplete)
                    && (!(applicationUser is TrangloStaff) ||
                        (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                        businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted && kycReviewResult != ReviewResult.Complete))
            {
                return Result.Failure<Declaration>($"Unable update declaration for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
            }

            if (_BusinessProfile.IsSuccess && _BusinessProfile.Value.Count > 0)
            {
                var DeclarationInfo = await _businessProfileService.GetKYCDeclarationInfoAsync(request.BusinessProfileCode);
                if (DeclarationInfo == null)

                {
                    return Result.Failure<Declaration>(
                                $"No declaration info found for {request.BusinessProfileCode}."
                            );
                }

                try
                {
                    DeclarationInfo.IsAgreedTermsOfService = request.IsAgreedTermsOfService;
                    DeclarationInfo.IsAuthorized = request.IsAuthorized;
                    DeclarationInfo.IsDeclareTransactionTax = request.IsDeclareTransactionTax;
                    DeclarationInfo.IsInformationTrue = request.IsInformationTrue;
                    DeclarationInfo.IsAllApplicationAccurate = request.IsAllApplicationAccurate;
                    DeclarationInfo.SigneeName = request.SigneeName;
                    DeclarationInfo.Designation = request.Designation;

                    var result = await _businessProfileService.UpdateKYCDeclarationInfoAsync(businessProfile,DeclarationInfo);
                    if (result != null)
                    {
                        return Result.Success<Declaration>(result);
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Unable to create declaration for BusinessProfileCode {0}, due to {1}", request.BusinessProfileCode, ex.Message);
                    _logger.LogError($"[UpdateDeclarationInformationCommand] {message}");
                }
            }

            else
            {
                _logger.LogError($"[UpdateDeclarationInformationCommand] BusinessProfileCode: {request.BusinessProfileCode} not found.");
                return Result.Failure<Declaration>($"No declaration info found for: {request.BusinessProfileCode}.");
            }

            return Result.Failure<Declaration>($"Unable update declaration for BusinessProfileCode: {request.BusinessProfileCode}");

        }
    }
}
