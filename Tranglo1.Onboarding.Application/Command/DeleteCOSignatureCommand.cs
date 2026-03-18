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
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAgreement, UACAction.Edit)]
    internal class DeleteCOSignatureCommand : BaseCommand<Result<COInformation>>
    {
        public int BusinessProfileCode { get; set; }
        public Guid CoSignatureDocumentId { get; set; }
        public string CoSignatureDocumentName { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }


        public override Task<string> GetAuditLogAsync(Result<COInformation> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Remove CO signature for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class DeleteCOSignatureCommandHandler : IRequestHandler<DeleteCOSignatureCommand, Result<COInformation>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<DeleteCOSignatureCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly StorageManager _storageManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public DeleteCOSignatureCommandHandler(BusinessProfileService businessProfileService, ILogger<DeleteCOSignatureCommandHandler> logger, TrangloUserManager userManager, StorageManager storageManager,
            PartnerService partnerService, IBusinessProfileRepository businessProfileRepository)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _userManager = userManager;
            _storageManager = storageManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<Result<COInformation>> Handle(DeleteCOSignatureCommand request, CancellationToken cancellationToken)
        {
            var COInfo = await _businessProfileService.GetCOInfoByBusinessCode(request.BusinessProfileCode);
            if (COInfo == null)

            {
                return Result.Failure<COInformation>(
                            $"S No record found for {request.BusinessProfileCode}."
                        );
            }
            
            COInfo.CoSignatureDocumentId = Guid.Empty;
            COInfo.COSignatureDocumentName = null;

            var businessProfilesResult = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(COInfo.BusinessProfileCode);
            BusinessProfile businessProfile = businessProfilesResult.Value;
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_ComplianceInfo.Id);

            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);


            if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
            {

                if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft || kycReviewResult != ReviewResult.Insufficient_Incomplete)
                    && (!(applicationUser is TrangloStaff) ||
                        (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                        businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted && kycReviewResult != ReviewResult.Complete))
                {
                    return Result.Failure<COInformation>($"Unable to delete signature document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
                }

                var update = await DeleteCOSignature(request, businessProfile, COInfo);

                if (update.IsFailure)
                {
                    return Result.Failure<COInformation>(
                                        $"Customer user is unable to delete CO signature document as {update.Error}"
                                        );
                }

                return update;
            }

            else if (ClaimCode.Business == request.CustomerSolution || Solution.Business.Id == request.AdminSolution)
            {
                var update = await DeleteCOSignature(request, businessProfile, COInfo);

                if (update.IsFailure)
                {
                    return Result.Failure<COInformation>(
                                        $"Business user is unable to delete CO signature document as {update.Error}"
                                        );
                }
                return update;
            }
            else
            {
                return Result.Failure<COInformation>($"Unable to delete signature document for BusinessProfileCode : {request.BusinessProfileCode}. Check Failure");
            }
        }

        private async Task<Result<COInformation>> DeleteCOSignature(DeleteCOSignatureCommand request, BusinessProfile businessProfile, COInformation COInfo)
        {
            //Remove document
            var document = await _storageManager.GetDocumentMetadataAsync(request.CoSignatureDocumentId);
            if (document == null)
            {
                return Result.Failure<COInformation>(
                            $"S No record found for {request.CoSignatureDocumentId}."
                        );
            }
            await _storageManager.RemoveAsync(request.CoSignatureDocumentId);

            //Update COInformation related columns
            Result<COInformation> UpdateCOInformationResp = await _businessProfileService.UpdateCOInformationsAsync(businessProfile, COInfo);
            if (UpdateCOInformationResp.IsFailure)
            {
                _logger.LogError($"[DeleteCOSignatureCommand] {UpdateCOInformationResp.Error}");

                return Result.Failure<COInformation>(
                            $"Delete signature Compliance Officers Info document failed for {request.BusinessProfileCode}."
                        );
            }
            return Result.Success(UpdateCOInformationResp.Value);
        }
    }
}
