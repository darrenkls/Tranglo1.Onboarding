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
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    internal class SaveCOSignatureCommand : BaseCommand<Result<Guid>>
    {
        public int BusinessProfileCode { get; set; }
        public IFormFile uploadedFile { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }


        public override Task<string> GetAuditLogAsync(Result<Guid> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add CO Signature Document for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveCOSignatureCommandHandler : IRequestHandler<SaveCOSignatureCommand, Result<Guid>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveCOSignatureCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;

        public SaveCOSignatureCommandHandler(BusinessProfileService businessProfileService,
                                                      ILogger<SaveCOSignatureCommandHandler> logger,
                                                      StorageManager storageManager,
                                                      TrangloUserManager userManager,
                                                      PartnerService partnerService,
                                                      IBusinessProfileRepository businessProfileRepository
                                                      )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
        }

        public async Task<Result<Guid>> Handle(SaveCOSignatureCommand request, CancellationToken cancellationToken)
        {
            var businessProfileList = await _businessProfileService.GetBusinessProfileByBusinessProfileCodeAsync(request.BusinessProfileCode);
            BusinessProfile businessProfile = businessProfileList.Value;
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_ComplianceInfo.Id);

            if (ClaimCode.Connect == request.CustomerSolution || Solution.Connect.Id == request.AdminSolution)
            {
                if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft || kycReviewResult != ReviewResult.Insufficient_Incomplete)
                    && (!(applicationUser is TrangloStaff) ||
                        (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                        businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted && kycReviewResult != ReviewResult.Complete))
                {
                    return Result.Failure<Guid>($"Unable to upload signature document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
                }

                var result = await SaveCOSignature(request, businessProfile, cancellationToken);
                //return error
                if (result.IsFailure)
                {
                    return Result.Failure<Guid>(
                                        $"Customer user is unable to save CO signature document for {request.BusinessProfileCode}."
                                        );
                }
                return Result.Success<Guid>(result.Value);
            }
            else if (ClaimCode.Business == request.CustomerSolution || Solution.Business.Id == request.AdminSolution)
            {
                var result = await SaveCOSignature(request, businessProfile, cancellationToken);
                //return error
                if (result.IsFailure)
                {
                    return Result.Failure<Guid>(
                                        $"Business user is unable to save CO signature document for {request.BusinessProfileCode}."
                                        );
                }
                return Result.Success<Guid>(result.Value);
            }
            else
            {
                return Result.Failure<Guid>($"Upload CO signature document is not available for {request.CustomerSolution}."
                                        );
            }
              
        }

        private async Task<Result<Guid>> SaveCOSignature(SaveCOSignatureCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            try
            {
                using (var ms = new MemoryStream())
                {

                    {
                        var extension = Path.GetExtension(request.uploadedFile.FileName)?.ToLower();
                        var allowedExtensions = new[] { ".jpg",".jpeg",".doc", ".docx", ".xls", ".xlsx", ".zip", ".pdf", ".png"};

                        var fileSize = request.uploadedFile.Length;
                        int maxFileSizeMB = 30;
                        var maxFileSize = maxFileSizeMB * 1024 * 1024;

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
                            
                            var COInfo = await _businessProfileService.GetCOInfoByBusinessCode(request.BusinessProfileCode);
                            if (COInfo != null)
                            {
                                COInfo.CoSignatureDocumentId = doc.DocumentId;
                                COInfo.COSignatureDocumentName = doc.FileName;
                                await _businessProfileService.UpdateCOInformationsAsync(businessProfile, COInfo);
                            }
                            else
                            {
                                COInformation coInformation = new COInformation(businessProfile)
                                {
                                    CoSignatureDocumentId = doc.DocumentId,
                                    COSignatureDocumentName = doc.FileName,
                                };
                                await _businessProfileService.AddCOInformationsAsync(businessProfile, coInformation);
                            }
                            return Result.Success<Guid>(doc.DocumentId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SaveCOSignatureCommand] {ex.Message}");
            }

            return Result.Failure<Guid>("Unable to upload document.");
        }
    }
}
