using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Infrastructure.Repositories;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    [Permission(Permission.KYCManagementDocumentation.Action_Comment_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business},
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class UploadCommentDocumentsCommand : BaseCommand<Result<Guid>>
    {
        public long documentcommentBPCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public long RequestId { get; set; }
        public IFormFile uploadedFile { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<Guid> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Review Remarks Document for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }

    }
    internal class UploadReviewRemarksDocumentsCommandHandler : IRequestHandler<UploadCommentDocumentsCommand, Result<Guid>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveDocumentCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public UploadReviewRemarksDocumentsCommandHandler(BusinessProfileService businessProfileService,
                                                      ILogger<SaveDocumentCommandHandler> logger,
                                                      StorageManager storageManager,
                                                      TrangloUserManager userManager,
                                                      PartnerService partnerService,
                                                      IBusinessProfileRepository businessProfileRepository,
                                                      IPartnerRepository partnerRepository
                                                      )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
            _partnerRepository = partnerRepository;
        }

        public async Task<Result<Guid>> Handle(UploadCommentDocumentsCommand request, CancellationToken cancellationToken)
        {
            var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
            BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            //var partnerSubscriptionInfo = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerRegistrationInfo.Id);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Documentation.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Documentation.Id);

            //check if partner is Business Partner
            if (request.AdminSolution != null || request.CustomerSolution != null) { 
                if (Solution.Connect.Id == request.AdminSolution || ClaimCode.Connect == request.CustomerSolution)

                    {
                        if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft ||
                        (kycReviewResult != ReviewResult.Insufficient_Incomplete || kycBusinessReviewResult != null))

                        && (!(applicationUser is TrangloStaff) ||
                            (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                            businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted &&
                            (kycReviewResult != ReviewResult.Complete || kycBusinessReviewResult != null)))
                        {
                            return Result.Failure<Guid>($"Unable save document for BusinessProfileCode {request.BusinessProfileCode}. Check Failure");
                        }
                    }
                }
            else
            {
                return Result.Failure<Guid>($"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
            }

            var documentCommentBPUploadInfo = await _businessProfileService.GetReviewRemarkByCommentCode(request.documentcommentBPCode);
            if (documentCommentBPUploadInfo != null)
            {
                return Result.Failure<Guid>(
                                        $"Unable to save document for BusinessProfileCode {request.BusinessProfileCode}. File already exists."
                                        );
            }

            try
            {
                using (var ms = new MemoryStream())
                {

                    {
                        var extension = Path.GetExtension(request.uploadedFile.FileName)?.ToLower();
                        var allowedExtensions = new[] { ".jpg",".jpeg",".xls", ".xlsx", ".doc", ".docx", ".pdf", ".png", ".zip" };
                        int maxFileSizeMB = 0;
                        var maxFileSize = 0;
                        var fileSize = request.uploadedFile.Length;
                        
                             maxFileSizeMB = 15;
                             maxFileSize = maxFileSizeMB * 1024 * 1024;
                        
                        if (fileSize > maxFileSize)
                        {
                            return Result.Failure<Guid>($"Document file size exceeds {maxFileSizeMB}mb");
                        }
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Result.Failure<Guid>("Document extension is not allowed");
                        }

                        await request.uploadedFile.CopyToAsync(ms);
                        ms.Position = 0;
                        var doc = await _storageManager.StoreAsync(ms, request.uploadedFile.FileName, request.uploadedFile.ContentType);
                        if (doc != null)
                        {
                            {
                                DocumentCommentUploadBP documentCommentUpload = new DocumentCommentUploadBP()
                                {
                                    DocumentId = doc.DocumentId,
                                    DocumentCommentBPCode = request.documentcommentBPCode,
                                };


                                await _businessProfileService.AddDocumentCommentUploadBP(documentCommentUpload);
                                return Result.Success<Guid>(doc.DocumentId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UploadCommentDocumentsStackTrace: {ex.StackTrace}", ex.Message);
            }

            return Result.Failure<Guid>("Unable to upload document.");
        }

    }

}
