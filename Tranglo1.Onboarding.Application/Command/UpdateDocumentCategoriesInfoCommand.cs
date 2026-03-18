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
    [Permission(Permission.KYCManagementDocumentation.Action_ReviewResult_Code,
        new int[] { (int)PortalCode.Admin },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class UpdateDocumentCategoriesInfoCommand : BaseCommand<Result<DocumentCategoryBP>>
    {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public long DocumentCategoryBPStatusCode { get; set; }
        public string LoginId { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<DocumentCategoryBP> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Update Document Categories for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }
    internal class UpdateDocumentCategoriesInfoCommandHandler : IRequestHandler<UpdateDocumentCategoriesInfoCommand, Result<DocumentCategoryBP>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<UpdateDocumentCategoriesInfoCommandHandler> _logger;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public UpdateDocumentCategoriesInfoCommandHandler(BusinessProfileService businessProfileService,
                                                         ILogger<UpdateDocumentCategoriesInfoCommandHandler> logger,
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

        public async Task<Result<DocumentCategoryBP>> Handle(UpdateDocumentCategoriesInfoCommand request, CancellationToken cancellationToken)
        {
            var _BusinessProfile = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
            BusinessProfile businessProfile = _BusinessProfile.Value.FirstOrDefault();
            var documentCategoryList = await _businessProfileService.GetCategoryInfoByCategoryCodeAsync(request.DocumentCategoryCode);
            DocumentCategory documentCategoryInfo = documentCategoryList.Value.FirstOrDefault();

            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var partnerSubscriptionInfo = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerRegistrationInfo.Id);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Documentation.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);

            if(request.AdminSolution != null) { 
                if (Solution.Connect.Id == request.AdminSolution)
                {
                    if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft ||
                        (kycReviewResult != ReviewResult.Insufficient_Incomplete)) //|| kycBusinessReviewResult != null))

                        && (!(applicationUser is TrangloStaff) ||
                            (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                            businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted &&
                            (kycReviewResult != ReviewResult.Complete))) // || kycBusinessReviewResult != null)))
                    {
                        return Result.Failure<DocumentCategoryBP>($"Unable update document category for categoryId: {request.DocumentCategoryCode}. Check Failure");
                    }
                }
            }
            else
            {
                return Result.Failure<DocumentCategoryBP>($"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
            }

            if (_BusinessProfile.IsSuccess && _BusinessProfile.Value.Count > 0)
            {
                var DocumentCategoryInfo = await _businessProfileService.GetDocumentCategoryBPAsync(request.DocumentCategoryCode, request.BusinessProfileCode);
                if (DocumentCategoryInfo == null)

                {
                    DocumentCategoryBP CategoryBP = new DocumentCategoryBP(businessProfile, documentCategoryInfo);

                    Result<DocumentCategoryBP> addCategoryBPResp = await _businessProfileService.AddCategoryBPAsync(CategoryBP);
                    DocumentCategoryInfo = addCategoryBPResp.Value;
                }

                try
                {
                    DocumentCategoryInfo.DocumentCategoryBPStatusCode = request.DocumentCategoryBPStatusCode;


                    var result = await _businessProfileService.UpdateDocumentCategoryBPInfo(DocumentCategoryInfo);
                    if (result != null)
                    {
                        return Result.Success<DocumentCategoryBP>(result);
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Unable to update document category for categoryId {0}, due to {1}", request.DocumentCategoryCode, ex.Message);
                    _logger.LogError($"[UpdateDocumentCategoriesInfoCommand] {message}");
                }
            }

            else
            {
                _logger.LogError($"[UpdateDocumentCategoryInfoCommand] BusinessProfileCode: {request.BusinessProfileCode} not found.");
                return Result.Failure<DocumentCategoryBP>($"No document category info found for: {request.DocumentCategoryCode}.");
            }

            return Result.Failure<DocumentCategoryBP>($"Unable update document category for categoryId: {request.DocumentCategoryCode}");

        }
    }
}
