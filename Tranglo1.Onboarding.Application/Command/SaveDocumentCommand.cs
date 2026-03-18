using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    [Permission(Permission.KYCManagementDocumentation.Action_Upload_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class SaveDocumentCommand : BaseCommand<Result<Guid>>
    {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public long RequestId { get; set; }
        public IFormFile uploadedFile { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public bool FromComment { get; set; }

        public override Task<string> GetAuditLogAsync(Result<Guid> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Document for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class SaveDocumentCommandHandler : IRequestHandler<SaveDocumentCommand, Result<Guid>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<SaveDocumentCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly PartnerService _partnerService;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public SaveDocumentCommandHandler(BusinessProfileService businessProfileService,
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

        public async Task<Result<Guid>> Handle(SaveDocumentCommand request, CancellationToken cancellationToken)
        {
            var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
            BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var partnerSubscriptionInfo = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistrationInfo.Id);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Documentation.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Ownership.Id);
            var documentCategoryInfo = await _businessProfileRepository.GetDocumentCategoriesAsync(request.DocumentCategoryCode);
            var isConnectExist = partnerSubscriptionInfo.Exists(x => x.Solution.Id == Solution.Connect.Id);
            var isBusinessExist = partnerSubscriptionInfo.Exists(x => x.Solution.Id == Solution.Business.Id);
            var seqNo = 0;

            Result<Guid> result = new Result<Guid>();

            if (request.AdminSolution != null || request.CustomerSolution != null)
            {
                if (ClaimCode.Connect == request.CustomerSolution)
                {
                    if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft &&
                        (kycReviewResult == ReviewResult.Insufficient_Incomplete || kycBusinessReviewResult != null))
                    {
                        if (isConnectExist && isBusinessExist)
                        {
                            //For TC 
                            result = await SaveDocument(request, businessProfile, cancellationToken);
                            //return error
                            if (result.IsFailure)
                            {
                                return Result.Failure<Guid>($"{result.Error}");
                            }

                            //For TB
                            seqNo = documentCategoryInfo.SequenceNo;
                            //checking document category of sequence No is 14, no need to upload to TB .
                            if (seqNo != 14)
                            {
                                if (seqNo != 1)
                                {
                                    seqNo = seqNo - 1;
                                }
                                var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTBAsync(seqNo);

                                SaveDocumentCommand saveDocumentCommand = new SaveDocumentCommand
                                {
                                    DocumentCategoryCode = (int)dcc.Id,
                                    BusinessProfileCode = request.BusinessProfileCode,
                                    uploadedFile = request.uploadedFile,
                                    LoginId = request.LoginId,
                                    RequestId = request.RequestId,
                                    AdminSolution = request.AdminSolution
                                };

                                result = await SaveDocument(saveDocumentCommand, businessProfile, cancellationToken);
                                //return error
                                if (result.IsFailure)
                                {
                                    return Result.Failure<Guid>($"{result.Error}");
                                }
                            }
                        }
                        else
                        {
                            //only 1 subscription
                            //update
                            result = await SaveDocument(request, businessProfile, cancellationToken);
                            //return error
                            if (result.IsFailure)
                            {
                                return Result.Failure<Guid>($"{result.Error}");
                            }
                        }
                    }
                }
                else if (ClaimCode.Business == request.CustomerSolution)
                {
                    if (isConnectExist && isBusinessExist)
                    {
                        //For TB 
                        result = await SaveDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<Guid>($"{result.Error}");
                        }

                        //For TC
                        seqNo = documentCategoryInfo.SequenceNo;
                        //checking document category of sequence No is 13, no need to upload to TC .
                        if (seqNo != 13)
                        {
                            if (seqNo != 1)
                            {
                                seqNo = seqNo + 1;
                            }
                            var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTCAsync(seqNo);

                            SaveDocumentCommand saveDocumentCommand = new SaveDocumentCommand
                            {
                                DocumentCategoryCode = (int)dcc.Id,
                                BusinessProfileCode = request.BusinessProfileCode,
                                uploadedFile = request.uploadedFile,
                                LoginId = request.LoginId,
                                RequestId = request.RequestId,
                                AdminSolution = request.AdminSolution
                            };

                            result = await SaveDocument(saveDocumentCommand, businessProfile, cancellationToken);
                            //return error
                            if (result.IsFailure)
                            {
                                return Result.Failure<Guid>($"{result.Error}");
                            }
                        }
                    }
                    else
                    {
                        //only 1 subscription
                        //update
                        result = await SaveDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<Guid>($"{result.Error}");
                        }

                        //Update Collection Tier if Document is attached
                        var documentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPAsync(request.DocumentCategoryCode, request.BusinessProfileCode);
                        //Individu
                        if (partnerRegistrationInfo.CustomerTypeCode == CustomerType.Individual.Id)
                        {
                            if (documentCategoryBPs.DocumentCategoryCode == 18)
                            {
                                var updateCollectionTierOnDocumentation = await _businessProfileService.EnsureCollectionTierOnDocumentation(businessProfile, true);
                            }
                        }
                        //Corporate
                        else if (partnerRegistrationInfo.CustomerTypeCode == CustomerType.Corporate_Normal_Corporate.Id)
                        {
                            if (documentCategoryBPs.DocumentCategoryCode == 27)
                            {
                                var updateCollectionTierOnDocumentation = await _businessProfileService.EnsureCollectionTierOnDocumentation(businessProfile, true);
                            }
                        }
                    }

                    await MarkKYCSummaryNotificationsAsReadAsync(request.BusinessProfileCode, KYCCategory.Business_Documentation.Id, cancellationToken);
                }

                else if (Solution.Connect.Id == request.AdminSolution)
                {
                    if (((applicationUser is TrangloStaff && bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                    businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted ||
                    (kycReviewResult == ReviewResult.Complete || kycBusinessReviewResult != null)))
                    {
                        if (isConnectExist && isBusinessExist)
                        {
                            //For TC 
                            result = await SaveDocument(request, businessProfile, cancellationToken);
                            //return error
                            if (result.IsFailure)
                            {
                                return Result.Failure<Guid>($"{result.Error}");
                            }

                            //For TB
                            seqNo = documentCategoryInfo.SequenceNo;
                            //checking document category of sequence No is 14, no need to upload to TB .
                            if (seqNo != 14)
                            {
                                if (seqNo != 1)
                                {
                                    seqNo = seqNo - 1;
                                }
                                var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTBAsync(seqNo);

                                SaveDocumentCommand saveDocumentCommand = new SaveDocumentCommand
                                {
                                    DocumentCategoryCode = (int)dcc.Id,
                                    BusinessProfileCode = request.BusinessProfileCode,
                                    uploadedFile = request.uploadedFile,
                                    LoginId = request.LoginId,
                                    RequestId = request.RequestId,
                                    AdminSolution = request.AdminSolution
                                };

                                result = await SaveDocument(saveDocumentCommand, businessProfile, cancellationToken);
                                //return error
                                if (result.IsFailure)
                                {
                                    return Result.Failure<Guid>($"{result.Error}");
                                }
                            }
                        }
                        else
                        {
                            //only 1 subscription
                            //update
                            result = await SaveDocument(request, businessProfile, cancellationToken);
                            //return error
                            if (result.IsFailure)
                            {
                                return Result.Failure<Guid>($"{result.Error}");
                            }
                        }
                    }

                }
                else if (Solution.Business.Id == request.AdminSolution)
                {
                    if (isConnectExist && isBusinessExist)
                    {
                        //For TB 
                        result = await SaveDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<Guid>($"{result.Error}");
                        }

                        //For TC
                        seqNo = documentCategoryInfo.SequenceNo;
                        //checking document category of sequence No is 13, no need to upload to TC .
                        if (seqNo != 13)
                        {
                            if (seqNo != 1)
                            {
                                seqNo = seqNo + 1;
                            }
                            var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTCAsync(seqNo);

                            SaveDocumentCommand saveDocumentCommand = new SaveDocumentCommand
                            {
                                DocumentCategoryCode = (int)dcc.Id,
                                BusinessProfileCode = request.BusinessProfileCode,
                                uploadedFile = request.uploadedFile,
                                LoginId = request.LoginId,
                                RequestId = request.RequestId,
                                AdminSolution = request.AdminSolution
                            };

                            result = await SaveDocument(saveDocumentCommand, businessProfile, cancellationToken);
                            //return error
                            if (result.IsFailure)
                            {
                                return Result.Failure<Guid>($"Customer user is unable to save document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure. {result.Error}");
                            }
                        }
                    }
                    else
                    {
                        //only 1 subscription
                        //update
                        result = await SaveDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<Guid>($"Customer user is unable to save document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure. {result.Error}");
                        }

                        //Update Collection Tier if Document is attached
                        var documentCategoryBPs = await _businessProfileRepository.GetDocumentCategoryBPAsync(request.DocumentCategoryCode, request.BusinessProfileCode);
                        //Individu
                        if (partnerRegistrationInfo.CustomerTypeCode == CustomerType.Individual.Id)
                        {

                            if (documentCategoryBPs.DocumentCategoryCode == 18)
                            {
                                var updateCollectionTierOnDocumentation = await _businessProfileService.EnsureCollectionTierOnDocumentation(businessProfile, true);
                            }

                        }
                        //Corporate
                        else if (partnerRegistrationInfo.CustomerTypeCode == CustomerType.Corporate_Normal_Corporate.Id)
                        {
                            if (documentCategoryBPs.DocumentCategoryCode == 27)
                            {
                                var updateCollectionTierOnDocumentation = await _businessProfileService.EnsureCollectionTierOnDocumentation(businessProfile, true);
                            }
                        }
                    }
                }
            }
            else
            {
                return Result.Failure<Guid>($"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
            }
            return result;
        }

        private async Task<Result<Guid>> SaveDocument(SaveDocumentCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            var documentCategoryList = await _businessProfileService.GetCategoryInfoByCategoryCodeAsync(request.DocumentCategoryCode);
            DocumentCategory documentCategoryInfo = documentCategoryList.Value.FirstOrDefault();

            var checkCategoryBP = await _businessProfileService.GetDocumentCategoryAsync(request.BusinessProfileCode, request.DocumentCategoryCode);
            if (checkCategoryBP == null)
            {
                DocumentCategoryBP CategoryBP = new DocumentCategoryBP(businessProfile, documentCategoryInfo);

                var addCategoryBPResp = await _businessProfileService.AddCategoryBPAsync(CategoryBP);
                checkCategoryBP = addCategoryBPResp.Value;
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    var extension = Path.GetExtension(request.uploadedFile.FileName)?.ToLower();
                    var allowedExtensions = new[] { ".xls", ".xlsx", ".doc", ".docx", ".pdf", ".png", ".jpg", ".jpeg" };

                    var fileSize = request.uploadedFile.Length;
                    int maxFileSizeMB = 30;
                    var maxFileSize = maxFileSizeMB * 1024 * 1024;

                    if (fileSize > maxFileSize)
                    {
                        return Result.Failure<Guid>($"Document file size exceeds {maxFileSizeMB}mb");
                    }
                    if (!allowedExtensions.Contains(extension))
                    {
                        return Result.Failure<Guid>($"This file format isn’t supported. Please upload a valid document type.");
                    }

                    await request.uploadedFile.CopyToAsync(ms);
                    ms.Position = 0;
                    var doc = await _storageManager.StoreAsync(ms, request.uploadedFile.FileName, request.uploadedFile.ContentType);
                    if (doc != null)
                    {
                        {
                            DocumentUploadBP documentUpload = new DocumentUploadBP()
                            {
                                DocumentId = doc.DocumentId,
                                DocumentCategoryBP = checkCategoryBP,
                            };

                            await _businessProfileService.AddDocumentUploadAsync(businessProfile, documentUpload);

                            checkCategoryBP.DocumentCategoryBPStatusCode = 2;
                            await _businessProfileService.UpdateDocumentCategoryBP(checkCategoryBP);
                            return Result.Success<Guid>(doc.DocumentId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SaveDocumentCommand] {ex} StackTrace: {ex.StackTrace}", ex.Message);
            }

            return Result.Failure<Guid>("Unable to upload document.");
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
                _logger.LogError(ex, "[{0}]", nameof(SaveDocumentCommandHandler));
            }
        }
    }
}