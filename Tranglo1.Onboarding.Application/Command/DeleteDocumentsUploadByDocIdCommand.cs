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
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.DocumentStorage;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    [Permission(Permission.KYCManagementDocumentation.Action_Remove_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    internal class DeleteDocumentsUploadByDocIdCommand : BaseCommand<Result<DocumentUploadBP>>
    {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public Guid DocumentId { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<DocumentUploadBP> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Delete Documents Upload for Document ID: [{this.DocumentId}] and Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class DeleteDocumentsUploadByDocIdCommandHandler : IRequestHandler<DeleteDocumentsUploadByDocIdCommand,Result<DocumentUploadBP>>
    {
        private readonly BusinessProfileService _businessProfileService;
        private readonly PartnerService _partnerService;
        private readonly ILogger<DeleteDocumentsUploadByDocIdCommandHandler> _logger;
        private readonly StorageManager _storageManager;
        private readonly TrangloUserManager _userManager;
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly IPartnerRepository _partnerRepository;

        public DeleteDocumentsUploadByDocIdCommandHandler(BusinessProfileService businessProfileService,
                                                      ILogger<DeleteDocumentsUploadByDocIdCommandHandler> logger,
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

        public async Task<Result<DocumentUploadBP>> Handle(DeleteDocumentsUploadByDocIdCommand request, CancellationToken cancellationToken)
        {
            var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
            BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();
            var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
            var partnerSubscriptionInfo = await _partnerRepository.GetSubscriptionsByPartnerCodeAsync(partnerRegistrationInfo.Id);
            ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
            var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
            var documentCategoryInfo = await _businessProfileRepository.GetDocumentCategoriesAsync(request.DocumentCategoryCode);

            var isConnectExist = partnerSubscriptionInfo.Exists(x => x.Solution == Solution.Connect);
            var isBusinessExist = partnerSubscriptionInfo.Exists(x => x.Solution == Solution.Business);


            Result<DocumentUploadBP> result = new Result<DocumentUploadBP>();
            var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Documentation.Id);
            var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Documentation.Id);
            var seqNo = 0;
            if (ClaimCode.Connect == request.CustomerSolution)
            {
                if (applicationUser is CustomerUser && businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Draft && (kycReviewResult == ReviewResult.Insufficient_Incomplete
                    || kycBusinessReviewResult != null))
                {
                    if (isConnectExist == true && isBusinessExist == true)
                    {
                        //For TC
                        result = await DeleteDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<DocumentUploadBP>(
                                                $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                                );
                        }

                        //For TB
                        seqNo = documentCategoryInfo.SequenceNo;
                        if (seqNo != 1)
                        {
                            seqNo = seqNo - 1;
                        }
                        var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTBAsync(seqNo);
                        var dcbp = await _businessProfileService.GetDocumentCategoryBPAsync(dcc.Id, request.BusinessProfileCode);
                        var dcu = await _businessProfileService.GetDocumentUploadByIdAsync(dcbp.Id); 

                        DeleteDocumentsUploadByDocIdCommand deleteDocumentsUploadByDocIdCommand = new DeleteDocumentsUploadByDocIdCommand
                        {
                            DocumentCategoryCode = (int)dcc.Id,
                            BusinessProfileCode = request.BusinessProfileCode,
                            DocumentId = dcu.DocumentId,
                            LoginId = request.LoginId,
                            CustomerSolution = request.CustomerSolution,
                            AdminSolution = request.AdminSolution
                        };

                        result = await DeleteDocument(deleteDocumentsUploadByDocIdCommand, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<DocumentUploadBP>(
                                                $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                                );
                        }

                        //check mandatory fields
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                    }   
                    else
                    {
                        //update
                        result = await DeleteDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<DocumentUploadBP>(
                                                $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                                );
                        }

                        //check mandatory fields
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                    }



                }
            }
            else if (ClaimCode.Business == request.CustomerSolution)
            {
                if (isConnectExist == true && isBusinessExist == true)
                {
                    //For TB
                    result = await DeleteDocument(request, businessProfile, cancellationToken);
                    //return error
                    if (result.IsFailure)
                    {
                        return Result.Failure<DocumentUploadBP>(
                                            $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                            );
                    }

                    //For TC
                    seqNo = documentCategoryInfo.SequenceNo;
                    if (seqNo != 1)
                    {
                        seqNo = seqNo + 1;
                    }
                    var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTCAsync(seqNo);
                    var dcbp = await _businessProfileService.GetDocumentCategoryBPAsync(dcc.Id, request.BusinessProfileCode);
                    var dcu = await _businessProfileService.GetDocumentUploadByIdAsync(dcbp.Id);
                    DeleteDocumentsUploadByDocIdCommand deleteDocumentsUploadByDocIdCommand = new DeleteDocumentsUploadByDocIdCommand
                    {
                        DocumentCategoryCode = (int)dcc.Id,
                        BusinessProfileCode = request.BusinessProfileCode,
                        DocumentId = dcu.DocumentId,
                        LoginId = request.LoginId,
                        CustomerSolution = request.CustomerSolution,
                        AdminSolution = request.AdminSolution
                    };

                    result = await DeleteDocument(deleteDocumentsUploadByDocIdCommand, businessProfile, cancellationToken);
                    //return error
                    if (result.IsFailure)
                    {
                        return Result.Failure<DocumentUploadBP>(
                                            $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                            );
                    }

                    //check mandatory fields
                    await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                }
                else
                {
                    //update
                    result = await DeleteDocument(request, businessProfile, cancellationToken);
                    //return error
                    if (result.IsFailure)
                    {
                        return Result.Failure<DocumentUploadBP>(
                                            $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                            );
                    }

                    //check mandatory fields
                    await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                }
            }
            else if (Solution.Connect.Id == request.AdminSolution)
            {
                if (applicationUser is TrangloStaff &&
                  ((bilateralPartnerFlow == PartnerType.Supply_Partner || bilateralPartnerFlow != null) ||
                  businessProfile.KYCSubmissionStatus == KYCSubmissionStatus.Submitted || (kycReviewResult == ReviewResult.Complete
                  || kycBusinessReviewResult != null)))
                {
                    if (isConnectExist == true && isBusinessExist == true)
                    {
                        //For TC
                        result = await DeleteDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<DocumentUploadBP>(
                                                $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                                );
                        }

                        //For TB
                        seqNo = documentCategoryInfo.SequenceNo;
                        if (seqNo != 1)
                        {
                            seqNo = seqNo - 1;
                        }
                        var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTBAsync(seqNo);
                        var dcbp = await _businessProfileService.GetDocumentCategoryBPAsync(dcc.Id, request.BusinessProfileCode);
                        var dcu = await _businessProfileService.GetDocumentUploadByIdAsync(dcbp.Id);
                        DeleteDocumentsUploadByDocIdCommand deleteDocumentsUploadByDocIdCommand = new DeleteDocumentsUploadByDocIdCommand
                        {
                            DocumentCategoryCode = (int)dcc.Id,
                            BusinessProfileCode = request.BusinessProfileCode,
                            DocumentId = dcu.DocumentId,
                            LoginId = request.LoginId,
                            CustomerSolution = request.CustomerSolution,
                            AdminSolution = request.AdminSolution
                        };

                        result = await DeleteDocument(deleteDocumentsUploadByDocIdCommand, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<DocumentUploadBP>(
                                                $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                                );
                        }

                        //check mandatory fields
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                    }
                    else
                    {
                        //update
                        result = await DeleteDocument(request, businessProfile, cancellationToken);
                        //return error
                        if (result.IsFailure)
                        {
                            return Result.Failure<DocumentUploadBP>(
                                                $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                                );
                        }

                        //check mandatory fields
                        await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                    }

                }
            }
            else if (Solution.Business.Id == request.AdminSolution) 
            {
                if (isConnectExist == true && isBusinessExist == true)
                {
                    //For TB
                    result = await DeleteDocument(request, businessProfile, cancellationToken);
                    //return error
                    if (result.IsFailure)
                    {
                        return Result.Failure<DocumentUploadBP>(
                                            $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                            );
                    }

                    //For TC
                    seqNo = documentCategoryInfo.SequenceNo;
                    if (seqNo != 1)
                    {
                        seqNo = seqNo + 1;
                    }
                    var dcc = await _businessProfileRepository.GetDocumentCategoriesMappingTCAsync(seqNo);
                    var dcbp = await _businessProfileService.GetDocumentCategoryBPAsync(dcc.Id, request.BusinessProfileCode);
                    var dcu = await _businessProfileService.GetDocumentUploadByIdAsync(dcbp.Id);
                    DeleteDocumentsUploadByDocIdCommand deleteDocumentsUploadByDocIdCommand = new DeleteDocumentsUploadByDocIdCommand
                    {
                        DocumentCategoryCode = (int)dcc.Id,
                        BusinessProfileCode = request.BusinessProfileCode,
                        DocumentId = request.DocumentId,
                        LoginId = request.LoginId,
                        CustomerSolution = request.CustomerSolution,
                        AdminSolution = request.AdminSolution
                    };

                    result = await DeleteDocument(deleteDocumentsUploadByDocIdCommand, businessProfile, cancellationToken);
                    //return error
                    if (result.IsFailure)
                    {
                        return Result.Failure<DocumentUploadBP>(
                                            $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                            );
                    }

                    //check mandatory fields
                    await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                }
                else
                {
                    //update
                    result = await DeleteDocument(request, businessProfile, cancellationToken);
                    //return error
                    if (result.IsFailure)
                    {
                        return Result.Failure<DocumentUploadBP>(
                                            $"Customer user is unable to delete document for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure"
                                            );
                    }

                    //check mandatory fields
                    await _businessProfileService.UpdateReviewResultIfMandatoryNotFilled(businessProfile, KYCCategory.Connect_Documentation);
                }
            }
            else
            {
                return Result.Failure<DocumentUploadBP>(
                                         $"Unable to delete document for BusinessProfileCode BusinessProfileCode: {request.BusinessProfileCode}."
                                         );
            }

            return result;
        }

        private async Task<Result<DocumentUploadBP>> DeleteDocument(DeleteDocumentsUploadByDocIdCommand request, BusinessProfile businessProfile, CancellationToken cancellationToken)
        {
            var documentCategoryBP = await _businessProfileService.GetDocumentCategoryAsync(request.BusinessProfileCode, request.DocumentCategoryCode);
            var documentUploadBP = await _businessProfileService.GetDocumentUploadByIdAsync(documentCategoryBP.Id, request.DocumentId);
            var documents = await _storageManager.GetDocumentMetadataAsync(request.DocumentId);

            if (documentUploadBP == null || documents == null)
            {
                return Result.Failure<DocumentUploadBP>(
                            $"No record found for {request.DocumentCategoryCode}."
                        );
            }

            if (documentUploadBP.IsVerified == true)
            {
                return Result.Failure<DocumentUploadBP>(
                            $"Unable to delete as the document is already verified for documentId {request.DocumentId}."
                        );
            }

            var deleteDocumentUploadBP = await _businessProfileService.DeleteDocumentUploadBP(businessProfile, documentUploadBP);
            await _storageManager.RemoveAsync(documents.DocumentId);

            var checkDocumentUploadBP = await _businessProfileService.GetDocumentUploadByIdAsync(documentCategoryBP.Id);

            if (checkDocumentUploadBP == null)
            {
                documentCategoryBP.DocumentCategoryBPStatusCode = 1;
                await _businessProfileService.UpdateDocumentCategoryBP(documentCategoryBP);
            }

            return Result.Success(documentUploadBP);
        }
    }    
}
