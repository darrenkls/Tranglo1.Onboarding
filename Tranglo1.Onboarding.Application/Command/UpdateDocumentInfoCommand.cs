using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
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
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Approve)]
    [Permission(Permission.KYCManagementDocumentation.Action_ReviewVerify_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class UpdateDocumentInfoCommand : BaseCommand<Result<DocumentUploadBP>>
    {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public Guid documentId { get; set; }
        public bool IsVerified { get; set; }
        public string LoginId { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<DocumentUploadBP> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Document Info for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }
    internal class UpdateDocumentInfoCommandHandler : IRequestHandler<UpdateDocumentInfoCommand, Result<DocumentUploadBP>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<UpdateDocumentInfoCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;
        public UpdateDocumentInfoCommandHandler(BusinessProfileService businessProfileService,
                                                ILogger<UpdateDocumentInfoCommandHandler> logger,
                                                TrangloUserManager userManager,
                                                PartnerService partnerService,
                                                IBusinessProfileRepository businessProfileRepository,
                                                IPartnerRepository partnerRepository)
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<DocumentUploadBP>> Handle(UpdateDocumentInfoCommand request, CancellationToken cancellationToken)
        {

            var _BusinessProfile = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
            BusinessProfile businessProfile = _BusinessProfile.Value.FirstOrDefault();
            var documentCategoryList = await _businessProfileService.GetCategoryInfoByCategoryCodeAsync(request.DocumentCategoryCode);
            DocumentCategory documentCategoryInfo = documentCategoryList.Value.FirstOrDefault();
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            //var partnerSubscriptionInfo = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerRegistrationInfo.Id);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Documentation.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Documentation.Id);

            //check if partner is Business Partner
            if(request.AdminSolution != null) { 
                if (Solution.Connect.Id == request.AdminSolution)
                {
                    if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft ||
                        (kycReviewResult != ReviewResult.Insufficient_Incomplete)) // || kycBusinessReviewResult != null))

                            && (!(applicationUser is TrangloStaff) ||
                                (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                                businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted && (kycReviewResult != ReviewResult.Complete))) //|| kycBusinessReviewResult != null)))
                    {
                        return Result.Failure<DocumentUploadBP>($"Unable update document for documentId: {request.documentId}. Check Failure");

                    }
                }
            }
            else
            {
                return Result.Failure<DocumentUploadBP>($"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
            }



            var documentCategoryBP = await _businessProfileService.GetDocumentCategoryBPAsync(request.DocumentCategoryCode, request.BusinessProfileCode);

            if (documentCategoryBP == null)
            {
                DocumentCategoryBP CategoryBP = new DocumentCategoryBP(businessProfile, documentCategoryInfo);

                Result<DocumentCategoryBP> addCategoryBPResp = await _businessProfileService.AddCategoryBPAsync(CategoryBP);
                documentCategoryBP = addCategoryBPResp.Value;
            }

            if (_BusinessProfile.IsSuccess && _BusinessProfile.Value.Count > 0)
            {
                var DocumentInfo = await _businessProfileService.GetDocumentUploadByIdAsync(documentCategoryBP.Id,request.documentId);
                if (DocumentInfo == null)

                {
                    return Result.Failure<DocumentUploadBP>(
                                $"No document info found for {request.documentId}."
                            );
                }

                try
                {
                    DocumentInfo.IsVerified = request.IsVerified;

                    var result = await _businessProfileService.UpdateDocumentUploadBP(DocumentInfo);
                    if (result != null)
                    {
                        return Result.Success<DocumentUploadBP>(result);
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Unable to update for DocumentId {0}, due to {1}", request.documentId, ex.Message);
                    _logger.LogError($"[UpdateDocumentInfoCommand] {message}");
                }
            }

            else
            {
                _logger.LogError($"[UpdateDocumentInfoCommand] DocumentId: {request.documentId} not found.");
                return Result.Failure<DocumentUploadBP>($"No document info found for: {request.documentId}.");
            }

            return Result.Failure<DocumentUploadBP>($"Unable update document for documentId: {request.documentId}");

        }
    }
}
