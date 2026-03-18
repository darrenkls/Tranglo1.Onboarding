using CSharpFunctionalExtensions;
using FluentValidation.Internal;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
    //[Permission(PermissionGroupCode.KYCDeclaration, UACAction.Edit)]
    internal class AddDeclarationSignatureCommand : BaseCommand<Result<Guid>>
    {
        public int BusinessProfileCode { get; set; }
        public IFormFile uploadedFile { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(Result<Guid> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Declarations Signature for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class AddDeclarationSignatureCommandHandler : IRequestHandler<AddDeclarationSignatureCommand, Result<Guid>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<AddDeclarationSignatureCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IConfiguration _config;
        public AddDeclarationSignatureCommandHandler(BusinessProfileService businessProfileService,
                                                      ILogger<AddDeclarationSignatureCommandHandler> logger,
                                                      StorageManager storageManager,
                                                      TrangloUserManager userManager,
                                                      PartnerService partnerService,
                                                      IBusinessProfileRepository businessProfileRepository,
                                                      IConfiguration config
                                                      )
        {
            _businessProfileService = businessProfileService;
            _logger = logger;
            _storageManager = storageManager;
            _userManager = userManager;
            _partnerService = partnerService;
            _businessProfileRepository = businessProfileRepository;
            _config = config;

        }

        public async Task<Result<Guid>> Handle(AddDeclarationSignatureCommand request, CancellationToken cancellationToken)
        {
            var _BusinessProfile = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var businessProfile = _BusinessProfile.Value?.FirstOrDefault();
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);

            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);

            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Declaration.Id);

            if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft || kycReviewResult != ReviewResult.Insufficient_Incomplete)
                    && (!(applicationUser is TrangloStaff) ||
                        (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null) &&
                        businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted && kycReviewResult != ReviewResult.Complete))
            {
                return Result.Failure<Guid>($"Unable update document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    var extension = Path.GetExtension(request.uploadedFile.FileName)?.ToLower();
                    var allowedExtensions = new[] { ".xls", ".xlsx", ".doc", ".docx", ".pdf", ".png", ".zip", ".jpg", ".jpeg", ".JPG", ".JPEG" };

                    var fileSize = request.uploadedFile.Length;
                    int maxFileSizeMB = 15;
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
                        var declaration = await _businessProfileService.GetKYCDeclarationInfoAsync(request.BusinessProfileCode);

                        if (declaration == null)
                        {
                            if (_BusinessProfile.IsSuccess && _BusinessProfile.Value != null)
                            {
                                var isTCRevampFeature = _config.GetValue<bool>("TCRevampFeature");
                                var result = await _businessProfileService.InsertKYCDeclarationInfoAsync(businessProfile, new Declaration(isTCRevampFeature)
                                {
                                    BusinessProfile = _BusinessProfile.Value.FirstOrDefault(),
                                    DocumentId = doc.DocumentId
                                });
                            }
                        }
                        else
                        {
                            declaration.DocumentId = doc.DocumentId;
                            await _businessProfileService.UpdateKYCDeclarationInfoAsync(businessProfile, declaration);

                        }
                        return Result.Success<Guid>(doc.DocumentId);
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"[AddDeclarationSignatureCommand] {ex.Message}");
            }

            return Result.Failure<Guid>("Unable to upload signature.");

        }
    }
}
