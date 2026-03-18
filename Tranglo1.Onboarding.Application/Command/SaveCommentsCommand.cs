using AutoMapper;
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
using Tranglo1.Onboarding.Infrastructure.Persistence;
using Tranglo1.UserAccessControl;


namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.KYCDocumentation, UACAction.Edit)]
    [Permission(Permission.KYCManagementDocumentation.Action_Comment_Code,
        new int[] { (int)PortalCode.Admin, (int)PortalCode.Connect, (int)PortalCode.Business },
        new string[] { Permission.KYCManagementDocumentation.Action_View_Code })]
    public class SaveCommentsCommand : BaseCommand<Result<long>>
    {
        public int DocumentCategoryCode { get; set; }
        public int BusinessProfileCode { get; set; }
        public string Comment { get; set; }
        public string LoginId { get; set; }
        public string CustomerSolution { get; set; }
        public long? AdminSolution { get; set; }

        public override Task<string> GetAuditLogAsync(Result<long> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Add Comments for Business Profile Code: [{this.BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
        public class SaveCommentsCommandHandler : IRequestHandler<SaveCommentsCommand, Result<long>>
        {
            private readonly BusinessProfileService _businessProfileService;
            private readonly BusinessProfileDbContext _context;
            private readonly IMapper _mapper;
            private readonly ILogger<SaveCommentsCommandHandler> _logger; 
            private readonly TrangloUserManager _userManager;
            private readonly PartnerService _partnerService;
            private readonly IBusinessProfileRepository _businessProfileRepository;
            private readonly IPartnerRepository _partnerRepository;

            public SaveCommentsCommandHandler(
              BusinessProfileService businessProfileService,
              ILogger<SaveCommentsCommandHandler> logger,
              BusinessProfileDbContext context, IMapper mapper,
              TrangloUserManager userManager,
              PartnerService partnerService,
              IBusinessProfileRepository businessProfileRepository,
              IPartnerRepository partnerRepository
          )
            {
                _businessProfileService = businessProfileService;
                _context = context;
                _mapper = mapper;
                _logger = logger;
                _userManager = userManager;
                _partnerService = partnerService;
                _businessProfileRepository = businessProfileRepository;
                _partnerRepository = partnerRepository;
            }

            public async Task<Result<long>> Handle(SaveCommentsCommand request, CancellationToken cancellationToken)
            {
                //Checking if comments length is more than 1500 characters
                if (request.Comment.Length > 1500)
                {
                    return Result.Failure<long>($"Unable save ReviewRemarks for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure as comments characters is more than 1500");
                }
                var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(request.BusinessProfileCode);
                BusinessProfile businessProfile = businessProfileList.Value.FirstOrDefault();
                var partnerRegistrationInfo = await _partnerService.GetPartnerRegistrationCodeByBusinessProfileCodeAsync(request.BusinessProfileCode);
                var partnerSubscriptionInfo = await _partnerRepository.GetPartnerSubscriptionListAsync(partnerRegistrationInfo.Id);
                var bilateralPartnerFlow = await _partnerService.GetPartnerFlow(partnerRegistrationInfo.Id);
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(request.LoginId);
                var kycReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Connect_Documentation.Id);
                var kycBusinessReviewResult = await _businessProfileRepository.GetReviewResultByCodeAsync(request.BusinessProfileCode, KYCCategory.Business_Documentation.Id);
                //checking if partner is business 

                if (request.AdminSolution != null || request.CustomerSolution != null)
                {
                    if (Solution.Connect.Id == request.AdminSolution)
                    {
                        if ((!(applicationUser is CustomerUser) || businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Draft ||
                        (kycReviewResult != ReviewResult.Insufficient_Incomplete || kycBusinessReviewResult != null))

                        && (!(applicationUser is TrangloStaff) || (bilateralPartnerFlow != PartnerType.Supply_Partner || bilateralPartnerFlow != null)
                        && businessProfile.KYCSubmissionStatus != KYCSubmissionStatus.Submitted
                        && (kycReviewResult != ReviewResult.Complete || kycBusinessReviewResult != null)))
                        {
                            return Result.Failure<long>($"Unable save comment for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
                        }
                    }
                }
                else 
                {
                    return Result.Failure<long>($"Solution Code passed is NULL for BusinessProfileCode: {request.BusinessProfileCode}. Check Failure");
                }


                

                var documentCategoryList = await _businessProfileService.GetCategoryInfoByCategoryCodeAsync(request.DocumentCategoryCode);
                DocumentCategory documentCategoryInfo = documentCategoryList.Value.FirstOrDefault();

                if (businessProfile == null || documentCategoryInfo == null)
                {
                    //To ensure that it returns 404-NotFound in the controller
                    return Result.Failure<long>("null");
                    /*
                    return Result.Failure<long>(
                                $"No document category info found for {request.DocumentCategoryCode}."
                            );    
                    */
                }

                var checkCategoryBP = await _businessProfileService.GetDocumentCategoryBPAsync(request.DocumentCategoryCode, request.BusinessProfileCode);
                if (checkCategoryBP == null)
                {
                    DocumentCategoryBP CategoryBP = new DocumentCategoryBP(businessProfile, documentCategoryInfo);

                    Result<DocumentCategoryBP> addCategoryBPResp = await _businessProfileService.AddCategoryBPAsync(CategoryBP);
                    checkCategoryBP = addCategoryBPResp.Value;
          
                }                
                    
                Result<DocumentCategoryBP> addCommentBPResp = await _businessProfileService.GetDocumentCategoryBPAsync(request.DocumentCategoryCode, request.BusinessProfileCode);

                DocumentCommentBP comments = new DocumentCommentBP()
                {
                    IsExternal = true,
                    DocumentCategoryBP = addCommentBPResp.Value,
                    Comment = request.Comment,
                };

               var addCommentsResp = await _businessProfileService.AddCommentsAsync(comments);
                if (addCommentsResp.IsFailure)
                {
                    _logger.LogError($"[SaveCommentsCommand] {addCommentsResp.Error}");

                    return Result.Failure<long>(
                                $"Save Comments failed for {request.DocumentCategoryCode}."
                            );
                }
                return Result.Success<long>(addCommentsResp.Value.Id);
                
            }
        }

    }


}